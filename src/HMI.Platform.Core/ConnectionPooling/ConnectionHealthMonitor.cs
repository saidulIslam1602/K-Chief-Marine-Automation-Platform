using System.Collections.Concurrent;

namespace HMI.Platform.Core.ConnectionPooling;

/// <summary>
/// Monitors connection health and statistics.
/// </summary>
/// <typeparam name="TConnection">Type of connection</typeparam>
public class ConnectionHealthMonitor<TConnection> where TConnection : class
{
    private readonly ConcurrentDictionary<string, ConnectionHealthStats> _stats = new();
    private readonly Timer _monitoringTimer;

    public ConnectionHealthMonitor(TimeSpan monitoringInterval)
    {
        _monitoringTimer = new Timer(CollectStats, null, monitoringInterval, monitoringInterval);
    }

    public void RecordAcquisition(string connectionId, TimeSpan duration)
    {
        var stats = _stats.GetOrAdd(connectionId, _ => new ConnectionHealthStats { ConnectionId = connectionId });
        stats.TotalAcquisitions++;
        stats.TotalAcquisitionTime += duration;
        stats.LastAcquiredAt = DateTime.UtcNow;
    }

    public void RecordReturn(string connectionId, TimeSpan duration)
    {
        var stats = _stats.GetOrAdd(connectionId, _ => new ConnectionHealthStats { ConnectionId = connectionId });
        stats.TotalReturns++;
        stats.TotalUsageTime += duration;
        stats.LastReturnedAt = DateTime.UtcNow;
    }

    public void RecordError(string connectionId, Exception exception)
    {
        var stats = _stats.GetOrAdd(connectionId, _ => new ConnectionHealthStats { ConnectionId = connectionId });
        stats.TotalErrors++;
        stats.LastErrorAt = DateTime.UtcNow;
        stats.LastError = exception.GetType().Name;
    }

    public ConnectionHealthStats GetStats(string connectionId)
    {
        return _stats.TryGetValue(connectionId, out var stats) ? stats : new ConnectionHealthStats { ConnectionId = connectionId };
    }

    public IEnumerable<ConnectionHealthStats> GetAllStats()
    {
        return _stats.Values;
    }

    private void CollectStats(object? state)
    {
        // Collect and aggregate statistics
        // This could send metrics to telemetry system
    }

    public void Dispose()
    {
        _monitoringTimer?.Dispose();
    }
}

/// <summary>
/// Connection health statistics.
/// </summary>
public class ConnectionHealthStats
{
    public string ConnectionId { get; set; } = string.Empty;
    public int TotalAcquisitions { get; set; }
    public int TotalReturns { get; set; }
    public int TotalErrors { get; set; }
    public TimeSpan TotalAcquisitionTime { get; set; }
    public TimeSpan TotalUsageTime { get; set; }
    public DateTime? LastAcquiredAt { get; set; }
    public DateTime? LastReturnedAt { get; set; }
    public DateTime? LastErrorAt { get; set; }
    public string? LastError { get; set; }

    public double AverageAcquisitionTime => TotalAcquisitions > 0 
        ? TotalAcquisitionTime.TotalMilliseconds / TotalAcquisitions 
        : 0;

    public double AverageUsageTime => TotalReturns > 0 
        ? TotalUsageTime.TotalMilliseconds / TotalReturns 
        : 0;

    public double ErrorRate => TotalAcquisitions > 0 
        ? (double)TotalErrors / TotalAcquisitions 
        : 0;
}

