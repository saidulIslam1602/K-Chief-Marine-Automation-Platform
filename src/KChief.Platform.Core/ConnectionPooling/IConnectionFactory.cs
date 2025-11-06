namespace KChief.Platform.Core.ConnectionPooling;

/// <summary>
/// Factory interface for creating connections.
/// </summary>
/// <typeparam name="TConnection">Type of connection</typeparam>
public interface IConnectionFactory<TConnection>
    where TConnection : class
{
    /// <summary>
    /// Creates a new connection.
    /// </summary>
    Task<TConnection> CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a connection.
    /// </summary>
    Task<bool> ValidateAsync(TConnection connection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disposes a connection.
    /// </summary>
    Task DisposeAsync(TConnection connection);
}

