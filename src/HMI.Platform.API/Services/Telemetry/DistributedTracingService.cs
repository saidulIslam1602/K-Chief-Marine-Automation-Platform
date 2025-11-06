using System.Diagnostics;
using HMI.Platform.Core.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace HMI.Platform.API.Services.Telemetry;

/// <summary>
/// Service for distributed tracing across services.
/// </summary>
public class DistributedTracingService
{
    private readonly ITelemetryService _telemetryService;
    private readonly TelemetryClient _telemetryClient;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<DistributedTracingService> _logger;

    public DistributedTracingService(
        ITelemetryService telemetryService,
        TelemetryClient telemetryClient,
        ILogger<DistributedTracingService> logger)
    {
        _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = new ActivitySource("HMI.DistributedTracing");
    }

    /// <summary>
    /// Starts a new trace span.
    /// </summary>
    public Activity? StartSpan(string operationName, string? parentId = null, IDictionary<string, string>? tags = null)
    {
        try
        {
            Activity? activity = null;

            if (!string.IsNullOrEmpty(parentId))
            {
                // Create activity with parent context
                if (ActivityContext.TryParse(parentId, null, out var parentContext))
                {
                    activity = _activitySource.StartActivity(operationName, ActivityKind.Server, parentContext);
                }
                else
                {
                    activity = _activitySource.StartActivity(operationName);
                }
            }
            else
            {
                activity = _activitySource.StartActivity(operationName);
            }

            if (activity != null)
            {
                // Add tags
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        activity.SetTag(tag.Key, tag.Value);
                    }
                }

                // Set W3C trace context
                activity.SetTag("trace.id", activity.TraceId.ToString());
                activity.SetTag("span.id", activity.SpanId.ToString());

                _logger.LogDebug("Started trace span: {OperationName} - TraceId: {TraceId}, SpanId: {SpanId}",
                    operationName, activity.TraceId, activity.SpanId);
            }

            return activity;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start trace span: {OperationName}", operationName);
            return null;
        }
    }

    /// <summary>
    /// Ends a trace span.
    /// </summary>
    public void EndSpan(Activity? activity, bool success = true, string? errorMessage = null)
    {
        if (activity == null)
        {
            return;
        }

        try
        {
            activity.SetTag("success", success.ToString());
            if (!string.IsNullOrEmpty(errorMessage))
            {
                activity.SetTag("error", errorMessage);
            }

            activity.Stop();

            _logger.LogDebug("Ended trace span: {OperationName} - TraceId: {TraceId}, SpanId: {SpanId}, Success: {Success}",
                activity.OperationName, activity.TraceId, activity.SpanId, success);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to end trace span: {OperationName}", activity.OperationName);
        }
    }

    /// <summary>
    /// Adds a tag to an activity.
    /// </summary>
    public void AddTag(Activity? activity, string key, string value)
    {
        if (activity == null)
        {
            return;
        }

        try
        {
            activity.SetTag(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add tag to activity: {Key}", key);
        }
    }

    /// <summary>
    /// Adds an event to an activity.
    /// </summary>
    public void AddEvent(Activity? activity, string eventName, IDictionary<string, string>? attributes = null)
    {
        if (activity == null)
        {
            return;
        }

        try
        {
            // ActivityEvent doesn't support adding tags after creation
            // Instead, add the event with tags as activity tags
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    activity.SetTag($"{eventName}.{attr.Key}", attr.Value);
                }
            }
            
            var activityEvent = new ActivityEvent(eventName);
            activity.AddEvent(activityEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add event to activity: {EventName}", eventName);
        }
    }

    /// <summary>
    /// Gets the current trace ID.
    /// </summary>
    public string? GetCurrentTraceId()
    {
        return Activity.Current?.TraceId.ToString();
    }

    /// <summary>
    /// Gets the current span ID.
    /// </summary>
    public string? GetCurrentSpanId()
    {
        return Activity.Current?.SpanId.ToString();
    }

    /// <summary>
    /// Gets the trace context for propagation.
    /// </summary>
    public string? GetTraceContext()
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            return null;
        }

        return $"00-{activity.TraceId}-{activity.SpanId}-01";
    }
}

