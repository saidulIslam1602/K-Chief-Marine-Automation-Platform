namespace KChief.Platform.Core.ConnectionPooling;

/// <summary>
/// Options for connection pooling.
/// </summary>
public class ConnectionPoolOptions
{
    /// <summary>
    /// Maximum number of connections in the pool.
    /// </summary>
    public int MaxSize { get; set; } = 10;

    /// <summary>
    /// Minimum number of connections to maintain in the pool.
    /// </summary>
    public int MinSize { get; set; } = 2;

    /// <summary>
    /// Maximum time to wait for a connection to become available.
    /// </summary>
    public TimeSpan AcquireTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum lifetime of a connection before it's replaced.
    /// </summary>
    public TimeSpan? MaxLifetime { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Maximum idle time before a connection is removed from the pool.
    /// </summary>
    public TimeSpan? MaxIdleTime { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Interval for health check of connections.
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Timeout for connection creation.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether to validate connections before returning them from the pool.
    /// </summary>
    public bool ValidateOnReturn { get; set; } = true;

    /// <summary>
    /// Whether to validate connections before acquiring them from the pool.
    /// </summary>
    public bool ValidateOnAcquire { get; set; } = true;
}

