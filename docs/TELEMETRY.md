# Performance Metrics and Telemetry

## Overview

The HMI Marine Automation Platform includes comprehensive telemetry and performance monitoring capabilities using Application Insights, custom metrics, distributed tracing, and performance profiling.

## Architecture

### Telemetry Components

```
Telemetry System
├── Application Insights Integration
├── Custom Metrics Service (Counters, Gauges, Histograms)
├── Distributed Tracing Service
├── Performance Profiling Service
└── Telemetry Middleware
```

## Application Insights Integration

### Configuration

Application Insights is configured in `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-instrumentation-key-here"
  }
}
```

### Features Enabled

- **Adaptive Sampling**: Automatically adjusts sampling rate based on traffic
- **Dependency Tracking**: Tracks external dependencies (databases, APIs, etc.)
- **Request Tracking**: Tracks all HTTP requests
- **Event Counter Collection**: Collects .NET runtime metrics
- **Performance Counter Collection**: Collects system performance counters

### Automatic Telemetry

Application Insights automatically collects:

- **Request Telemetry**: HTTP requests, response times, status codes
- **Dependency Telemetry**: Database queries, external API calls
- **Exception Telemetry**: Unhandled exceptions with stack traces
- **Performance Counters**: CPU, memory, disk I/O
- **Custom Events**: Business events and metrics

## Custom Metrics Service

### Counters

Counters track cumulative values that only increase:

```csharp
// HTTP Requests Counter
kchief_http_requests_total{method="GET", endpoint="/api/vessels", status_code="200"}

// HTTP Errors Counter
kchief_http_errors_total{method="POST", endpoint="/api/vessels", status_code="500"}

// Vessel Operations Counter
kchief_vessel_operations_total{operation="StartEngine", vessel_id="vessel-001", success="true"}

// Alarm Triggers Counter
kchief_alarm_triggers_total{alarm_type="Temperature", severity="Warning", vessel_id="vessel-001"}

// Database Queries Counter
kchief_database_queries_total{operation="Select", table="Vessels", success="true"}
```

### Gauges

Gauges track values that can increase or decrease:

```csharp
// Active Vessels Gauge
kchief_active_vessels = 15

// Active Alarms Gauge
kchief_active_alarms = 3

// Active Connections Gauge
kchief_active_connections = 42

// Cache Hit Rate Gauge
kchief_cache_hit_rate = 85.5%
```

### Histograms

Histograms track distributions of values:

```csharp
// Request Duration Histogram
kchief_request_duration_seconds{method="GET", endpoint="/api/vessels"} = [0.1, 0.2, 0.5, 1.0, 2.0]

// Database Query Duration Histogram
kchief_database_query_duration_seconds{operation="Select", table="Vessels"} = [0.01, 0.05, 0.1, 0.2]

// Vessel Operation Duration Histogram
kchief_vessel_operation_duration_seconds{operation="StartEngine", vessel_id="vessel-001"} = [0.5, 1.0, 2.0]
```

### Usage Example

```csharp
public class VesselsController : ControllerBase
{
    private readonly CustomMetricsService _metricsService;

    public VesselsController(CustomMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public async Task<ActionResult> GetVessels()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var vessels = await _vesselService.GetAllAsync();
            stopwatch.Stop();

            _metricsService.RecordHttpRequest(
                "GET",
                "/api/vessels",
                200,
                stopwatch.Elapsed.TotalSeconds);

            return Ok(vessels);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metricsService.RecordHttpRequest(
                "GET",
                "/api/vessels",
                500,
                stopwatch.Elapsed.TotalSeconds);
            throw;
        }
    }
}
```

## Distributed Tracing

### W3C Trace Context

The platform supports W3C Trace Context for distributed tracing across services:

```
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
```

### Starting a Trace

```csharp
public class MyService
{
    private readonly DistributedTracingService _tracingService;

    public async Task ProcessAsync(string vesselId)
    {
        var activity = _tracingService.StartSpan(
            "ProcessVessel",
            parentId: null, // Use current context if null
            tags: new Dictionary<string, string>
            {
                ["vessel.id"] = vesselId,
                ["operation.type"] = "processing"
            });

        try
        {
            // Add events during processing
            _tracingService.AddEvent(activity, "ProcessingStarted");
            
            // Do work
            await DoWorkAsync(vesselId);
            
            _tracingService.AddEvent(activity, "ProcessingCompleted");
        }
        catch (Exception ex)
        {
            _tracingService.AddTag(activity, "error", "true");
            _tracingService.AddEvent(activity, "ProcessingFailed", new Dictionary<string, string>
            {
                ["error.message"] = ex.Message
            });
            throw;
        }
        finally
        {
            _tracingService.EndSpan(activity, success: true);
        }
    }
}
```

### Trace Propagation

Traces are automatically propagated via HTTP headers:

- **Request Header**: `traceparent` - Contains parent trace context
- **Response Header**: `traceresponse` - Contains current trace context

### Viewing Traces

Traces can be viewed in:
- Application Insights Portal
- Azure Monitor
- Custom dashboards using trace IDs

## Performance Profiling

### Profiling an Operation

```csharp
public class MyService
{
    private readonly PerformanceProfilingService _profilingService;

    public async Task<Result> ProcessAsync(string data)
    {
        return await _profilingService.ProfileAsync(
            "ProcessData",
            async () => await DoProcessAsync(data),
            metadata: new Dictionary<string, string>
            {
                ["data.size"] = data.Length.ToString(),
                ["operation.type"] = "processing"
            });
    }
}
```

### Manual Profiling

```csharp
var sessionId = _profilingService.StartProfiling("ComplexOperation");

_profilingService.RecordCheckpoint(sessionId, "Initialized");
await Step1Async();

_profilingService.RecordCheckpoint(sessionId, "Step1Completed");
await Step2Async();

_profilingService.RecordCheckpoint(sessionId, "Step2Completed");
var result = await Step3Async();

var profilingResult = _profilingService.StopProfiling(sessionId);

// Access profiling data
Console.WriteLine($"Duration: {profilingResult.Duration.TotalMilliseconds}ms");
foreach (var checkpoint in profilingResult.Checkpoints)
{
    Console.WriteLine($"{checkpoint.Name}: {checkpoint.ElapsedMilliseconds}ms");
}
```

### Profiling Results

Profiling results include:
- **Session ID**: Unique identifier for the profiling session
- **Operation Name**: Name of the profiled operation
- **Start/End Time**: Timestamps
- **Duration**: Total execution time
- **Checkpoints**: Intermediate timing points
- **Metadata**: Custom metadata

## Request Duration Tracking

### Automatic Tracking

The `TelemetryMiddleware` automatically tracks:
- Request method and endpoint
- Response status code
- Request duration
- Success/failure status
- Correlation ID

### Slow Request Detection

Requests taking longer than 2 seconds are automatically flagged:
- Logged as warnings
- Tracked as custom events in Application Insights
- Tagged in distributed traces

### Custom Tracking

```csharp
public class MyController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;

    [HttpPost]
    public async Task<ActionResult> CreateVessel([FromBody] CreateVesselRequest request)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var vessel = await _vesselService.CreateAsync(request);
            stopwatch.Stop();

            _telemetryService.TrackRequest(
                "POST /api/vessels",
                startTime,
                stopwatch.Elapsed,
                "201",
                success: true);

            return CreatedAtAction(nameof(GetVessel), new { id = vessel.Id }, vessel);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _telemetryService.TrackException(ex, new Dictionary<string, string>
            {
                ["endpoint"] = "/api/vessels",
                ["method"] = "POST"
            });
            throw;
        }
    }
}
```

## Custom Events

### Tracking Business Events

```csharp
_telemetryService.TrackEvent("VesselStarted", new Dictionary<string, string>
{
    ["vesselId"] = vesselId,
    ["engineCount"] = engineCount.ToString(),
    ["operator"] = operatorName
}, new Dictionary<string, double>
{
    ["startupTime"] = startupDuration.TotalSeconds
});
```

### Tracking Metrics

```csharp
_telemetryService.TrackMetric("ActiveVessels", vesselCount, new Dictionary<string, string>
{
    ["region"] = "north-atlantic"
});

_telemetryService.RecordGauge("MemoryUsage", memoryUsageMB, new Dictionary<string, string>
{
    ["server"] = Environment.MachineName
});
```

## Telemetry Service Interface

### ITelemetryService

The `ITelemetryService` interface provides a unified API for telemetry:

```csharp
public interface ITelemetryService
{
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
    void TrackMetric(string metricName, double value, IDictionary<string, string>? properties = null);
    void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success);
    void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
    void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);
    void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information);
    Activity? StartActivity(string name, string? parentId = null);
    void IncrementCounter(string counterName, IDictionary<string, string>? tags = null, double value = 1.0);
    void RecordGauge(string gaugeName, double value, IDictionary<string, string>? tags = null);
    void RecordHistogram(string histogramName, double value, IDictionary<string, string>? tags = null);
}
```

## Metrics Endpoint

### Prometheus-Compatible Metrics

The platform exposes metrics at `/metrics` endpoint in Prometheus format:

```
# HELP kchief_http_requests_total Total number of HTTP requests
# TYPE kchief_http_requests_total counter
kchief_http_requests_total{method="GET",endpoint="/api/vessels",status_code="200"} 1234

# HELP kchief_request_duration_seconds HTTP request duration in seconds
# TYPE kchief_request_duration_seconds histogram
kchief_request_duration_seconds_bucket{method="GET",endpoint="/api/vessels",le="0.1"} 1000
kchief_request_duration_seconds_bucket{method="GET",endpoint="/api/vessels",le="0.5"} 1200
kchief_request_duration_seconds_bucket{method="GET",endpoint="/api/vessels",le="1.0"} 1230
kchief_request_duration_seconds_bucket{method="GET",endpoint="/api/vessels",le="+Inf"} 1234
```

## Best Practices

### 1. Use Telemetry Service

Always use `ITelemetryService` for telemetry operations:

```csharp
// Good
_telemetryService.TrackEvent("VesselStarted", properties);

// Avoid
_telemetryClient.TrackEvent("VesselStarted", properties); // Direct dependency
```

### 2. Include Context

Always include relevant context in telemetry:

```csharp
_telemetryService.TrackEvent("VesselOperation", new Dictionary<string, string>
{
    ["vesselId"] = vesselId,
    ["operation"] = operationName,
    ["operator"] = operatorName,
    ["correlationId"] = correlationId
});
```

### 3. Profile Critical Operations

Profile operations that are:
- Performance-critical
- Frequently called
- Known to be slow
- Business-critical

### 4. Use Distributed Tracing

Use distributed tracing for:
- Cross-service operations
- Microservices communication
- Complex workflows
- Debugging production issues

### 5. Monitor Key Metrics

Monitor these key metrics:
- Request rate and latency
- Error rate
- Active connections
- Cache hit rate
- Database query performance
- Vessel operation success rate

## Configuration

### Application Insights

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key-here",
    "EnableAdaptiveSampling": true,
    "EnableDependencyTrackingTelemetryModule": true,
    "EnableRequestTrackingTelemetryModule": true,
    "EnableEventCounterCollectionModule": true,
    "EnablePerformanceCounterCollectionModule": true
  }
}
```

### Telemetry Options

```json
{
  "Telemetry": {
    "EnableDistributedTracing": true,
    "EnablePerformanceProfiling": true,
    "SlowRequestThresholdSeconds": 2.0,
    "MetricsCollectionIntervalSeconds": 30
  }
}
```

## Viewing Telemetry Data

### Application Insights Portal

1. Navigate to Azure Portal
2. Open Application Insights resource
3. View:
   - **Overview**: Key metrics and trends
   - **Performance**: Request performance analysis
   - **Failures**: Exception and error analysis
   - **Metrics**: Custom metrics and counters
   - **Live Metrics**: Real-time telemetry stream

### Custom Dashboards

Create custom dashboards using:
- Application Insights Analytics
- Power BI
- Grafana
- Custom visualization tools

### Query Examples

```kusto
// Requests by endpoint
requests
| summarize count() by name
| order by count_ desc

// Average request duration
requests
| summarize avg(duration) by name
| order by avg_duration desc

// Error rate
requests
| where success == false
| summarize count() by name, resultCode
| order by count_ desc

// Slow requests
requests
| where duration > 2000
| project timestamp, name, duration, resultCode
| order by duration desc
```

## Related Documentation

- [Monitoring Documentation](MONITORING.md)
- [Logging Documentation](LOGGING.md)
- [Architecture Documentation](ARCHITECTURE.md)
- [Developer Guide](DEVELOPER_GUIDE.md)

