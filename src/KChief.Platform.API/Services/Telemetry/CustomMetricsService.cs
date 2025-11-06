using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using KChief.Platform.Core.Telemetry;
using Microsoft.ApplicationInsights;

namespace KChief.Platform.API.Services.Telemetry;

/// <summary>
/// Service for managing custom metrics with Application Insights integration.
/// </summary>
public class CustomMetricsService
{
    private readonly ITelemetryService _telemetryService;
    private readonly TelemetryClient _telemetryClient;
    private readonly Meter _meter;
    private readonly ILogger<CustomMetricsService> _logger;

    // Counters
    private readonly Counter<long> _httpRequestsCounter;
    private readonly Counter<long> _httpErrorsCounter;
    private readonly Counter<long> _vesselOperationsCounter;
    private readonly Counter<long> _alarmTriggersCounter;
    private readonly Counter<long> _databaseQueriesCounter;

    // Gauges
    private readonly ObservableGauge<long> _activeVesselsGauge;
    private readonly ObservableGauge<long> _activeAlarmsGauge;
    private readonly ObservableGauge<long> _activeConnectionsGauge;
    private readonly ObservableGauge<double> _cacheHitRateGauge;

    // Histograms
    private readonly Histogram<double> _requestDurationHistogram;
    private readonly Histogram<double> _databaseQueryDurationHistogram;
    private readonly Histogram<double> _vesselOperationDurationHistogram;

    // Observable counters for business metrics
    private readonly ObservableCounter<long> _totalVesselsCounter;
    private readonly ObservableCounter<long> _totalEnginesCounter;

    // Custom metric values
    private long _activeVessels = 0;
    private long _activeAlarms = 0;
    private long _activeConnections = 0;
    private long _cacheHits = 0;
    private long _cacheMisses = 0;

    public CustomMetricsService(
        ITelemetryService telemetryService,
        TelemetryClient telemetryClient,
        ILogger<CustomMetricsService> logger)
    {
        _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _meter = new Meter("KChief.Metrics", "1.0.0");

        // Initialize counters
        _httpRequestsCounter = _meter.CreateCounter<long>(
            "kchief_http_requests_total",
            description: "Total number of HTTP requests");

        _httpErrorsCounter = _meter.CreateCounter<long>(
            "kchief_http_errors_total",
            description: "Total number of HTTP errors");

        _vesselOperationsCounter = _meter.CreateCounter<long>(
            "kchief_vessel_operations_total",
            description: "Total number of vessel operations");

        _alarmTriggersCounter = _meter.CreateCounter<long>(
            "kchief_alarm_triggers_total",
            description: "Total number of alarm triggers");

        _databaseQueriesCounter = _meter.CreateCounter<long>(
            "kchief_database_queries_total",
            description: "Total number of database queries");

        // Initialize gauges
        _activeVesselsGauge = _meter.CreateObservableGauge<long>(
            "kchief_active_vessels",
            observeValue: () => _activeVessels,
            description: "Number of active vessels");

        _activeAlarmsGauge = _meter.CreateObservableGauge<long>(
            "kchief_active_alarms",
            observeValue: () => _activeAlarms,
            description: "Number of active alarms");

        _activeConnectionsGauge = _meter.CreateObservableGauge<long>(
            "kchief_active_connections",
            observeValue: () => _activeConnections,
            description: "Number of active connections");

        _cacheHitRateGauge = _meter.CreateObservableGauge<double>(
            "kchief_cache_hit_rate",
            observeValue: GetCacheHitRate,
            unit: "%",
            description: "Cache hit rate percentage");

        // Initialize histograms
        _requestDurationHistogram = _meter.CreateHistogram<double>(
            "kchief_request_duration_seconds",
            unit: "s",
            description: "HTTP request duration in seconds");

        _databaseQueryDurationHistogram = _meter.CreateHistogram<double>(
            "kchief_database_query_duration_seconds",
            unit: "s",
            description: "Database query duration in seconds");

        _vesselOperationDurationHistogram = _meter.CreateHistogram<double>(
            "kchief_vessel_operation_duration_seconds",
            unit: "s",
            description: "Vessel operation duration in seconds");

        // Initialize observable counters
        _totalVesselsCounter = _meter.CreateObservableCounter<long>(
            "kchief_total_vessels",
            observeValue: () => GetTotalVessels(),
            description: "Total number of vessels");

        _totalEnginesCounter = _meter.CreateObservableCounter<long>(
            "kchief_total_engines",
            observeValue: () => GetTotalEngines(),
            description: "Total number of engines");

        _logger.LogInformation("Custom metrics service initialized");
    }

    /// <summary>
    /// Records an HTTP request.
    /// </summary>
    public void RecordHttpRequest(string method, string endpoint, int statusCode, double durationSeconds)
    {
        var tags = new TagList();
        tags.Add("method", method);
        tags.Add("endpoint", endpoint);
        tags.Add("status_code", statusCode.ToString());

        _httpRequestsCounter.Add(1, tags);
        _requestDurationHistogram.Record(durationSeconds, tags);

        if (statusCode >= 400)
        {
            _httpErrorsCounter.Add(1, tags);
        }

        // Also track in Application Insights
        _telemetryService.TrackMetric("HttpRequests", 1, new Dictionary<string, string>
        {
            ["method"] = method,
            ["endpoint"] = endpoint,
            ["status_code"] = statusCode.ToString()
        });

        _telemetryService.RecordHistogram("RequestDuration", durationSeconds, new Dictionary<string, string>
        {
            ["method"] = method,
            ["endpoint"] = endpoint
        });
    }

    /// <summary>
    /// Records a vessel operation.
    /// </summary>
    public void RecordVesselOperation(string operation, string vesselId, bool success, double durationSeconds)
    {
        var tags = new TagList();
        tags.Add("operation", operation);
        tags.Add("vessel_id", vesselId);
        tags.Add("success", success.ToString());

        _vesselOperationsCounter.Add(1, tags);
        _vesselOperationDurationHistogram.Record(durationSeconds, tags);

        _telemetryService.TrackEvent("VesselOperation", new Dictionary<string, string>
        {
            ["operation"] = operation,
            ["vesselId"] = vesselId,
            ["success"] = success.ToString()
        }, new Dictionary<string, double>
        {
            ["duration"] = durationSeconds
        });
    }

    /// <summary>
    /// Records an alarm trigger.
    /// </summary>
    public void RecordAlarmTrigger(string alarmType, string severity, string? vesselId = null)
    {
        var tags = new TagList();
        tags.Add("alarm_type", alarmType);
        tags.Add("severity", severity);
        if (!string.IsNullOrEmpty(vesselId))
        {
            tags.Add("vessel_id", vesselId);
        }

        _alarmTriggersCounter.Add(1, tags);

        _telemetryService.TrackEvent("AlarmTriggered", new Dictionary<string, string>
        {
            ["alarmType"] = alarmType,
            ["severity"] = severity,
            ["vesselId"] = vesselId ?? "unknown"
        });
    }

    /// <summary>
    /// Records a database query.
    /// </summary>
    public void RecordDatabaseQuery(string operation, string table, bool success, double durationSeconds)
    {
        var tags = new TagList();
        tags.Add("operation", operation);
        tags.Add("table", table);
        tags.Add("success", success.ToString());

        _databaseQueriesCounter.Add(1, tags);
        _databaseQueryDurationHistogram.Record(durationSeconds, tags);

        _telemetryService.TrackDependency("SQL", operation, table, DateTimeOffset.UtcNow, TimeSpan.FromSeconds(durationSeconds), success);
    }

    /// <summary>
    /// Updates active vessels count.
    /// </summary>
    public void UpdateActiveVessels(long count)
    {
        _activeVessels = count;
        _telemetryService.RecordGauge("ActiveVessels", count);
    }

    /// <summary>
    /// Updates active alarms count.
    /// </summary>
    public void UpdateActiveAlarms(long count)
    {
        _activeAlarms = count;
        _telemetryService.RecordGauge("ActiveAlarms", count);
    }

    /// <summary>
    /// Updates active connections count.
    /// </summary>
    public void UpdateActiveConnections(long count)
    {
        _activeConnections = count;
        _telemetryService.RecordGauge("ActiveConnections", count);
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    public void RecordCacheHit()
    {
        Interlocked.Increment(ref _cacheHits);
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    public void RecordCacheMiss()
    {
        Interlocked.Increment(ref _cacheMisses);
    }

    private double GetCacheHitRate()
    {
        var total = _cacheHits + _cacheMisses;
        if (total == 0)
        {
            return 0;
        }
        return (double)_cacheHits / total * 100;
    }

    private long GetTotalVessels()
    {
        // This would typically query the database or cache
        // For now, return a placeholder
        return _activeVessels;
    }

    private long GetTotalEngines()
    {
        // This would typically query the database or cache
        // For now, return a placeholder
        return 0;
    }
}

