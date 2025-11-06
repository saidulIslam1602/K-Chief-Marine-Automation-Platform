using System.Diagnostics;

namespace HMI.Platform.Core.Telemetry;

/// <summary>
/// Severity levels for trace messages.
/// </summary>
public enum TraceSeverity
{
    Verbose = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// Interface for telemetry and metrics collection.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Tracks a custom event.
    /// </summary>
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

    /// <summary>
    /// Tracks a custom metric.
    /// </summary>
    void TrackMetric(string metricName, double value, IDictionary<string, string>? properties = null);

    /// <summary>
    /// Tracks a dependency call.
    /// </summary>
    void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);

    /// <summary>
    /// Tracks an exception.
    /// </summary>
    void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

    /// <summary>
    /// Tracks a request.
    /// </summary>
    void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);

    /// <summary>
    /// Tracks a trace message.
    /// </summary>
    void TrackTrace(string message, TraceSeverity severityLevel = TraceSeverity.Information);

    /// <summary>
    /// Starts a new activity for distributed tracing.
    /// </summary>
    Activity? StartActivity(string name, string? parentId = null);

    /// <summary>
    /// Increments a counter metric.
    /// </summary>
    void IncrementCounter(string counterName, IDictionary<string, string>? tags = null, double value = 1.0);

    /// <summary>
    /// Records a gauge metric.
    /// </summary>
    void RecordGauge(string gaugeName, double value, IDictionary<string, string>? tags = null);

    /// <summary>
    /// Records a histogram metric.
    /// </summary>
    void RecordHistogram(string histogramName, double value, IDictionary<string, string>? tags = null);
}

