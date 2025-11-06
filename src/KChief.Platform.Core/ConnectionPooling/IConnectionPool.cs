namespace KChief.Platform.Core.ConnectionPooling;

/// <summary>
/// Interface for connection pooling.
/// </summary>
/// <typeparam name="TConnection">Type of connection</typeparam>
public interface IConnectionPool<TConnection> : IDisposable, IAsyncDisposable
    where TConnection : class
{
    /// <summary>
    /// Gets a connection from the pool.
    /// </summary>
    Task<PooledConnection<TConnection>> AcquireAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a connection to the pool.
    /// </summary>
    Task ReturnAsync(PooledConnection<TConnection> connection);

    /// <summary>
    /// Gets the current pool size.
    /// </summary>
    int CurrentSize { get; }

    /// <summary>
    /// Gets the maximum pool size.
    /// </summary>
    int MaxSize { get; }

    /// <summary>
    /// Gets the number of available connections.
    /// </summary>
    int AvailableCount { get; }

    /// <summary>
    /// Gets the number of active connections.
    /// </summary>
    int ActiveCount { get; }

    /// <summary>
    /// Clears the pool and disposes all connections.
    /// </summary>
    Task ClearAsync();
}

/// <summary>
/// Represents a pooled connection wrapper.
/// </summary>
/// <typeparam name="TConnection">Type of connection</typeparam>
public class PooledConnection<TConnection> : IDisposable, IAsyncDisposable
    where TConnection : class
{
    private readonly IConnectionPool<TConnection> _pool;
    private bool _returned = false;

    public TConnection Connection { get; }
    public DateTime AcquiredAt { get; }
    public string Id { get; }

    internal PooledConnection(TConnection connection, IConnectionPool<TConnection> pool)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        AcquiredAt = DateTime.UtcNow;
        Id = Guid.NewGuid().ToString("N")[..12];
    }

    public void Dispose()
    {
        if (!_returned)
        {
            _returned = true;
            _pool.ReturnAsync(this).GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_returned)
        {
            _returned = true;
            await _pool.ReturnAsync(this);
        }
    }
}

