namespace KChief.Platform.Core.ResourceManagement;

/// <summary>
/// Base class for async disposable resources.
/// </summary>
public abstract class AsyncDisposableBase : IAsyncDisposable, IDisposable
{
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await DisposeAsyncCore();
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DisposeManagedResources();
            }
            DisposeUnmanagedResources();
            _disposed = true;
        }
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        DisposeManagedResources();
        DisposeUnmanagedResources();
        return ValueTask.CompletedTask;
    }

    protected virtual void DisposeManagedResources()
    {
        // Override to dispose managed resources
    }

    protected virtual void DisposeUnmanagedResources()
    {
        // Override to dispose unmanaged resources
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}

