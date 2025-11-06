using System.Collections.Concurrent;

namespace KChief.Platform.Core.ResourceManagement;

/// <summary>
/// Manages resource disposal and tracking.
/// </summary>
public class ResourceManager : IAsyncDisposable, IDisposable
{
    private readonly ConcurrentBag<IAsyncDisposable> _asyncResources = new();
    private readonly ConcurrentBag<IDisposable> _syncResources = new();
    private bool _disposed = false;

    /// <summary>
    /// Registers an async disposable resource.
    /// </summary>
    public void Register(IAsyncDisposable resource)
    {
        if (resource == null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        ThrowIfDisposed();
        _asyncResources.Add(resource);
    }

    /// <summary>
    /// Registers a sync disposable resource.
    /// </summary>
    public void Register(IDisposable resource)
    {
        if (resource == null)
        {
            throw new ArgumentNullException(nameof(resource));
        }

        ThrowIfDisposed();
        _syncResources.Add(resource);
    }

    /// <summary>
    /// Registers a resource that implements both interfaces.
    /// </summary>
    public void Register(IAsyncDisposable asyncResource, IDisposable syncResource)
    {
        Register(asyncResource);
        Register(syncResource);
    }

    /// <summary>
    /// Disposes all registered resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Dispose async resources
        var asyncTasks = _asyncResources.Select(r => r.DisposeAsync().AsTask()).ToArray();
        await Task.WhenAll(asyncTasks);

        // Dispose sync resources
        foreach (var resource in _syncResources)
        {
            try
            {
                resource.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _asyncResources.Clear();
        _syncResources.Clear();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ResourceManager));
        }
    }
}

