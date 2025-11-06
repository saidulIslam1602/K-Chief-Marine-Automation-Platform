using System.Collections.Concurrent;
using System.Diagnostics;

namespace HMI.Platform.Core.ConnectionPooling;

/// <summary>
/// Base implementation of connection pool.
/// </summary>
/// <typeparam name="TConnection">Type of connection</typeparam>
public abstract class BaseConnectionPool<TConnection> : IConnectionPool<TConnection>
    where TConnection : class
{
    private readonly ConcurrentQueue<PooledConnectionItem> _availableConnections = new();
    private readonly ConcurrentDictionary<string, PooledConnectionItem> _activeConnections = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly ConnectionPoolOptions _options;
    private readonly Timer _healthCheckTimer;
    private readonly Timer _cleanupTimer;
    private bool _disposed = false;

    protected BaseConnectionPool(ConnectionPoolOptions? options = null)
    {
        _options = options ?? new ConnectionPoolOptions();
        _semaphore = new SemaphoreSlim(_options.MaxSize, _options.MaxSize);

        // Start health check timer
        _healthCheckTimer = new Timer(PerformHealthCheck, null, _options.HealthCheckInterval, _options.HealthCheckInterval);

        // Start cleanup timer (every minute)
        _cleanupTimer = new Timer(PerformCleanup, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public int CurrentSize => _availableConnections.Count + _activeConnections.Count;
    public int MaxSize => _options.MaxSize;
    public int AvailableCount => _availableConnections.Count;
    public int ActiveCount => _activeConnections.Count;

    public async Task<PooledConnection<TConnection>> AcquireAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        // Try to acquire semaphore
        var acquired = await _semaphore.WaitAsync(_options.AcquireTimeout, cancellationToken);
        if (!acquired)
        {
            throw new TimeoutException($"Failed to acquire connection from pool within {_options.AcquireTimeout}");
        }

        try
        {
            // Try to get an available connection
            if (_availableConnections.TryDequeue(out var pooledItem))
            {
                // Validate connection if required
                if (_options.ValidateOnAcquire && !await IsConnectionValidAsync(pooledItem.Connection, cancellationToken))
                {
                    // Connection is invalid, dispose it and create a new one
                    await DisposeConnectionAsync(pooledItem.Connection);
                    pooledItem = await CreatePooledItemAsync(cancellationToken);
                }
                else
                {
                    // Update last used time
                    pooledItem.LastUsed = DateTime.UtcNow;
                }

                _activeConnections[pooledItem.Id] = pooledItem;
                return new PooledConnection<TConnection>(pooledItem.Connection, this);
            }

            // No available connection, create a new one
            var newItem = await CreatePooledItemAsync(cancellationToken);
            _activeConnections[newItem.Id] = newItem;
            return new PooledConnection<TConnection>(newItem.Connection, this);
        }
        catch
        {
            _semaphore.Release();
            throw;
        }
    }

    public async Task ReturnAsync(PooledConnection<TConnection> connection)
    {
        if (connection == null)
        {
            return;
        }

        if (_activeConnections.TryRemove(connection.Id, out var pooledItem))
        {
            try
            {
                // Validate connection if required
                if (_options.ValidateOnReturn && !await IsConnectionValidAsync(pooledItem.Connection, CancellationToken.None))
                {
                    // Connection is invalid, dispose it
                    await DisposeConnectionAsync(pooledItem.Connection);
                }
                else
                {
                    // Return to pool
                    pooledItem.LastUsed = DateTime.UtcNow;
                    _availableConnections.Enqueue(pooledItem);
                }
            }
            catch
            {
                // If validation fails, dispose the connection
                await DisposeConnectionAsync(pooledItem.Connection);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public async Task ClearAsync()
    {
        // Dispose all available connections
        while (_availableConnections.TryDequeue(out var item))
        {
            await DisposeConnectionAsync(item.Connection);
        }

        // Wait for active connections to be returned
        var timeout = TimeSpan.FromSeconds(30);
        var stopwatch = Stopwatch.StartNew();
        while (_activeConnections.Count > 0 && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(100);
        }

        // Force dispose remaining active connections
        foreach (var item in _activeConnections.Values)
        {
            await DisposeConnectionAsync(item.Connection);
        }
        _activeConnections.Clear();
    }

    protected abstract Task<TConnection> CreateConnectionAsync(CancellationToken cancellationToken);
    protected abstract Task<bool> IsConnectionValidAsync(TConnection connection, CancellationToken cancellationToken);
    protected abstract Task DisposeConnectionAsync(TConnection connection);

    private async Task<PooledConnectionItem> CreatePooledItemAsync(CancellationToken cancellationToken)
    {
        var connection = await CreateConnectionAsync(cancellationToken);
        return new PooledConnectionItem
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Connection = connection,
            CreatedAt = DateTime.UtcNow,
            LastUsed = DateTime.UtcNow
        };
    }

    private async void PerformHealthCheck(object? state)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            var itemsToCheck = _availableConnections.ToArray();
            foreach (var item in itemsToCheck)
            {
                if (!await IsConnectionValidAsync(item.Connection, CancellationToken.None))
                {
                    // Remove invalid connection
                    if (_availableConnections.TryDequeue(out var removed) && removed.Id == item.Id)
                    {
                        await DisposeConnectionAsync(removed.Connection);
                    }
                }
            }
        }
        catch
        {
            // Ignore health check errors
        }
    }

    private async void PerformCleanup(object? state)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            var now = DateTime.UtcNow;
            var itemsToRemove = new List<PooledConnectionItem>();

            // Check for idle connections
            if (_options.MaxIdleTime.HasValue)
            {
                foreach (var item in _availableConnections)
                {
                    if (now - item.LastUsed > _options.MaxIdleTime.Value)
                    {
                        itemsToRemove.Add(item);
                    }
                }
            }

            // Check for expired connections
            if (_options.MaxLifetime.HasValue)
            {
                foreach (var item in _availableConnections)
                {
                    if (now - item.CreatedAt > _options.MaxLifetime.Value)
                    {
                        itemsToRemove.Add(item);
                    }
                }
            }

            // Remove items that exceed min size
            while (CurrentSize > _options.MinSize && itemsToRemove.Count > 0)
            {
                var item = itemsToRemove[0];
                itemsToRemove.RemoveAt(0);

                if (_availableConnections.TryDequeue(out var removed) && removed.Id == item.Id)
                {
                    await DisposeConnectionAsync(removed.Connection);
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(BaseConnectionPool<TConnection>));
        }
    }

    protected class PooledConnectionItem
    {
        public string Id { get; set; } = string.Empty;
        public TConnection Connection { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsed { get; set; }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _healthCheckTimer?.Dispose();
        _cleanupTimer?.Dispose();
        ClearAsync().GetAwaiter().GetResult();
        _semaphore?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _healthCheckTimer?.Dispose();
        _cleanupTimer?.Dispose();
        await ClearAsync();
        _semaphore?.Dispose();
    }
}

