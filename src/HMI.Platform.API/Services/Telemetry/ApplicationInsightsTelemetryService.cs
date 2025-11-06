using System.Diagnostics;
using HMI.Platform.Core.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace HMI.Platform.API.Services.Telemetry;

/// <summary>
/// Telemetry service implementation using Application Insights.
/// </summary>
public class ApplicationInsightsTelemetryService : ITelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ApplicationInsightsTelemetryService> _logger;
    private readonly ActivitySource _activitySource;

    public ApplicationInsightsTelemetryService(
        TelemetryClient telemetryClient,
        ILogger<ApplicationInsightsTelemetryService> logger)
    {
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = new ActivitySource("HMI.Platform.API");
    }

    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
    {
        try
        {
            _telemetryClient.TrackEvent(eventName, properties, metrics);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track event: {EventName}", eventName);
        }
    }

    public void TrackMetric(string metricName, double value, IDictionary<string, string>? properties = null)
    {
        try
        {
            _telemetryClient.TrackMetric(metricName, value, properties);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track metric: {MetricName}", metricName);
        }
    }

    public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
    {
        try
        {
            _telemetryClient.TrackDependency(dependencyTypeName, dependencyName, data, startTime, duration, success);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track dependency: {DependencyName}", dependencyName);
        }
    }

    public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
    {
        try
        {
            _telemetryClient.TrackException(exception, properties, metrics);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track exception: {ExceptionType}", exception.GetType().Name);
        }
    }

    public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
    {
        try
        {
            _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track request: {RequestName}", name);
        }
    }

    public void TrackTrace(string message, TraceSeverity severityLevel = TraceSeverity.Information)
    {
        try
        {
            var aiSeverity = severityLevel switch
            {
                TraceSeverity.Verbose => SeverityLevel.Verbose,
                TraceSeverity.Information => SeverityLevel.Information,
                TraceSeverity.Warning => SeverityLevel.Warning,
                TraceSeverity.Error => SeverityLevel.Error,
                TraceSeverity.Critical => SeverityLevel.Critical,
                _ => SeverityLevel.Information
            };
            _telemetryClient.TrackTrace(message, aiSeverity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track trace: {Message}", message);
        }
    }

    public Activity? StartActivity(string name, string? parentId = null)
    {
        try
        {
            Activity? activity = null;

            if (!string.IsNullOrEmpty(parentId))
            {
                // Create activity with parent context
                activity = _activitySource.StartActivity(name);
                if (activity != null && ActivityContext.TryParse(parentId, null, out var parentContext))
                {
                    activity.SetParentId(parentId);
                }
            }
            else
            {
                activity = _activitySource.StartActivity(name);
            }

            return activity;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start activity: {ActivityName}", name);
            return null;
        }
    }

    public void IncrementCounter(string counterName, IDictionary<string, string>? tags = null, double value = 1.0)
    {
        try
        {
            var metric = new MetricTelemetry(counterName, value);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    metric.Properties[tag.Key] = tag.Value;
                }
            }
            _telemetryClient.TrackMetric(metric);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to increment counter: {CounterName}", counterName);
        }
    }

    public void RecordGauge(string gaugeName, double value, IDictionary<string, string>? tags = null)
    {
        try
        {
            var metric = new MetricTelemetry(gaugeName, value);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    metric.Properties[tag.Key] = tag.Value;
                }
            }
            _telemetryClient.TrackMetric(metric);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record gauge: {GaugeName}", gaugeName);
        }
    }

    public void RecordHistogram(string histogramName, double value, IDictionary<string, string>? tags = null)
    {
        try
        {
            var metric = new MetricTelemetry(histogramName, value);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    metric.Properties[tag.Key] = tag.Value;
                }
            }
            _telemetryClient.TrackMetric(metric);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record histogram: {HistogramName}", histogramName);
        }
    }
}

