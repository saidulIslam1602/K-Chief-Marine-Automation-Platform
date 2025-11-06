# Structured Logging with Serilog

## Overview

The K-Chief Marine Automation Platform implements comprehensive structured logging using Serilog, providing rich, searchable, and actionable log data across all application components. This implementation demonstrates production-grade logging practices with correlation tracking, multiple sinks, and contextual enrichment.

## Architecture

### Logging Stack

```
Application Layer
    ↓
Serilog Logger (Structured)
    ↓
Multiple Sinks:
├── Console (Development/Production)
├── File (Rolling, Text & JSON)
├── Application Insights (Production)
└── Future: Elasticsearch, Seq, etc.
```

### Key Components

1. **Serilog Configuration** - Centralized logging configuration
2. **Correlation ID Middleware** - Request tracking across services
3. **Request/Response Logging** - Comprehensive HTTP logging
4. **Structured Error Logging** - Rich exception context
5. **Log Enrichment** - Automatic context addition

## Configuration

### Serilog Settings

The logging configuration is defined in `appsettings.json` with environment-specific overrides:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning",
        "KChief": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/kchief-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/kchief-structured-.json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithProcessId",
      "WithProcessName",
      "WithThreadId",
      "WithCorrelationId"
    ]
  }
}
```

### Environment-Specific Configuration

#### Development
- **Log Level**: Debug for K-Chief components, Information for Microsoft
- **Console Output**: Colored with correlation IDs
- **File Output**: Daily rolling files in `logs/dev/`
- **Retention**: 7 days

#### Production
- **Log Level**: Information for K-Chief, Warning for Microsoft
- **Console Output**: Structured without colors
- **File Output**: Daily rolling files in `/app/logs/`
- **Application Insights**: Enabled with telemetry
- **Retention**: 30 days

## Correlation ID Tracking

### CorrelationIdMiddleware

Ensures every request has a unique correlation ID for end-to-end tracking:

```csharp
public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);
        context.Items[CorrelationIdKey] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
        
        using (LogContext.PushProperty(CorrelationIdKey, correlationId))
        {
            await _next(context);
        }
    }
}
```

### Features

- **Header Detection**: Checks for existing correlation IDs in request headers
- **Alternative Headers**: Supports `X-Request-ID`, `X-Trace-ID`, `Request-ID`
- **Automatic Generation**: Creates short, readable IDs when none provided
- **Response Headers**: Adds correlation ID to response for client tracking
- **Log Context**: Automatically includes in all log entries

## Request/Response Logging

### RequestResponseLoggingMiddleware

Comprehensive HTTP request and response logging with structured data:

```csharp
public class RequestResponseLoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString();

        await LogRequestAsync(context, correlationId);
        
        // Capture response...
        
        await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);
    }
}
```

### Logged Information

#### Request Logging
- **HTTP Method & Path**
- **Query Parameters**
- **Client IP Address** (with proxy support)
- **User Agent**
- **Request Headers** (sanitized)
- **Request Body** (for supported content types, size-limited)
- **User Identity**

#### Response Logging
- **Status Code**
- **Response Time**
- **Content Type & Length**
- **Response Body** (for supported content types, size-limited)
- **Log Level** based on status code (Error for 5xx, Warning for 4xx)

### Content Filtering

- **Size Limits**: 10KB maximum for request/response bodies
- **Content Types**: Only logs JSON, XML, plain text, and form data
- **Security**: Automatically redacts sensitive headers (Authorization, Cookie, etc.)

## Structured Error Logging

### ErrorLoggingService

Enhanced error logging with rich contextual information:

```csharp
public void LogException(Exception exception, HttpContext? httpContext = null, string? correlationId = null)
{
    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("ExceptionType", exception.GetType().Name))
    {
        if (exception is KChiefException kchiefEx)
        {
            using (LogContext.PushProperty("ErrorCode", kchiefEx.ErrorCode))
            using (LogContext.PushProperty("ExceptionContext", kchiefEx.Context, destructureObjects: true))
            {
                LogExceptionWithSerilog(exception, correlationId, httpContext);
            }
        }
    }
}
```

### Features

- **Custom Exception Support**: Special handling for `KChiefException` types
- **Rich Context**: Includes error codes, custom context data
- **Request Context**: HTTP request details when available
- **Client Information**: IP address, user agent, user identity
- **Structured Data**: All context as searchable properties

## Log Enrichment

### Automatic Enrichers

The platform automatically enriches all log entries with:

- **Machine Name**: Server/container identifier
- **Environment Name**: Development, Staging, Production
- **Process ID & Name**: Application process information
- **Thread ID**: Execution thread for debugging
- **Correlation ID**: Request tracking identifier
- **Application Info**: Name and version

### Custom Properties

Services can add custom properties using `LogContext`:

```csharp
using (LogContext.PushProperty("VesselId", vesselId))
using (LogContext.PushProperty("Operation", "StartEngine"))
using (LogContext.PushProperty("UserId", userId))
{
    Log.Information("Engine started successfully for vessel {VesselId}", vesselId);
}
```

## Log Sinks

### Console Sink

**Development**: Colored output with full context
```
[14:23:45 INF] [a1b2c3d4] HTTP Request: GET /api/vessels from 192.168.1.100
```

**Production**: Structured output without colors
```
[14:23:45 INF] [a1b2c3d4] HTTP Request: GET /api/vessels from 192.168.1.100
```

### File Sinks

#### Text Files (`logs/kchief-YYYYMMDD.log`)
Human-readable format with full context:
```
2023-12-07 14:23:45.123 +00:00 [INF] [a1b2c3d4] HTTP Request: GET /api/vessels from 192.168.1.100 {"RequestMethod":"GET","RequestPath":"/api/vessels","ClientIP":"192.168.1.100"}
```

#### JSON Files (`logs/kchief-structured-YYYYMMDD.json`)
Machine-readable structured format:
```json
{
  "@t": "2023-12-07T14:23:45.123Z",
  "@l": "Information",
  "@mt": "HTTP Request: {RequestMethod} {RequestPath} from {ClientIP}",
  "CorrelationId": "a1b2c3d4",
  "RequestMethod": "GET",
  "RequestPath": "/api/vessels",
  "ClientIP": "192.168.1.100",
  "MachineName": "web-server-01",
  "ProcessId": 1234
}
```

### Application Insights Sink (Production)

Sends structured logs to Azure Application Insights for:
- **Real-time Monitoring**
- **Advanced Querying** with KQL
- **Alerting & Dashboards**
- **Correlation with Metrics**

## Usage Examples

### Basic Structured Logging

```csharp
public class VesselService
{
    public async Task<Vessel> GetVesselAsync(string vesselId)
    {
        using (LogContext.PushProperty("VesselId", vesselId))
        using (LogContext.PushProperty("Operation", "GetVessel"))
        {
            Log.Information("Retrieving vessel {VesselId}", vesselId);
            
            try
            {
                var vessel = await _repository.GetByIdAsync(vesselId);
                
                Log.Information("Successfully retrieved vessel {VesselId} of type {VesselType}", 
                    vesselId, vessel.Type);
                    
                return vessel;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve vessel {VesselId}", vesselId);
                throw;
            }
        }
    }
}
```

### Performance Logging

```csharp
public async Task<IActionResult> ProcessLargeOperation()
{
    using (LogContext.PushProperty("Operation", "ProcessLargeOperation"))
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Perform operation
            var result = await _service.ProcessAsync();
            
            stopwatch.Stop();
            Log.Information("Large operation completed successfully in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Large operation failed after {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

### Business Event Logging

```csharp
public async Task StartVesselAsync(string vesselId, string userId)
{
    using (LogContext.PushProperty("VesselId", vesselId))
    using (LogContext.PushProperty("UserId", userId))
    using (LogContext.PushProperty("BusinessEvent", "VesselStart"))
    {
        Log.Information("User {UserId} initiated start sequence for vessel {VesselId}", 
            userId, vesselId);
            
        // Perform operation
        
        Log.Information("Vessel {VesselId} successfully started by user {UserId}", 
            vesselId, userId);
    }
}
```

## Log Analysis and Monitoring

### Querying Structured Logs

#### Find all errors for a specific vessel:
```bash
grep '"VesselId":"vessel-001"' logs/kchief-structured-*.json | grep '"@l":"Error"'
```

#### Find slow requests (>1000ms):
```bash
jq 'select(.ElapsedMilliseconds > 1000)' logs/kchief-structured-*.json
```

#### Find all operations by correlation ID:
```bash
grep '"CorrelationId":"a1b2c3d4"' logs/kchief-*.log
```

### Application Insights Queries (KQL)

#### Error rate by vessel:
```kql
traces
| where customDimensions.VesselId != ""
| where severityLevel >= 3
| summarize ErrorCount = count() by tostring(customDimensions.VesselId)
| order by ErrorCount desc
```

#### Request performance analysis:
```kql
traces
| where message contains "HTTP Response"
| extend ElapsedMs = toint(customDimensions.ElapsedMilliseconds)
| summarize 
    AvgResponseTime = avg(ElapsedMs),
    P95ResponseTime = percentile(ElapsedMs, 95),
    RequestCount = count()
    by bin(timestamp, 5m)
```

#### User activity tracking:
```kql
traces
| where customDimensions.UserId != ""
| summarize 
    Operations = dcount(customDimensions.Operation),
    LastActivity = max(timestamp)
    by tostring(customDimensions.UserId)
```

## Best Practices

### 1. Structured Properties

```csharp
// Good: Structured properties
Log.Information("Vessel {VesselId} engine {EngineId} RPM changed to {NewRPM}", 
    vesselId, engineId, newRpm);

// Bad: String interpolation
Log.Information($"Vessel {vesselId} engine {engineId} RPM changed to {newRpm}");
```

### 2. Consistent Property Names

```csharp
// Use consistent property names across the application
using (LogContext.PushProperty("VesselId", vesselId))  // Always "VesselId"
using (LogContext.PushProperty("UserId", userId))      // Always "UserId"
using (LogContext.PushProperty("Operation", "StartEngine")) // Always "Operation"
```

### 3. Appropriate Log Levels

- **Verbose/Debug**: Detailed diagnostic information
- **Information**: General application flow
- **Warning**: Unexpected but recoverable situations
- **Error**: Error conditions that don't stop the application
- **Fatal**: Critical errors that cause application termination

### 4. Performance Considerations

```csharp
// Good: Use structured logging efficiently
if (Log.IsEnabled(LogEventLevel.Debug))
{
    Log.Debug("Detailed debug info: {@ComplexObject}", complexObject);
}

// Good: Avoid expensive operations in log messages
Log.Information("Processing {ItemCount} items", items.Count);

// Bad: Expensive serialization always executed
Log.Debug("Processing items: {Items}", JsonSerializer.Serialize(items));
```

### 5. Sensitive Data Protection

```csharp
// Good: Exclude sensitive data
Log.Information("User {UserId} authenticated successfully", user.Id);

// Bad: Logging sensitive information
Log.Information("User authenticated: {@User}", user); // May contain passwords, etc.
```

## Monitoring and Alerting

### Key Metrics to Monitor

1. **Error Rate**: Percentage of requests resulting in errors
2. **Response Time**: P95 response times by endpoint
3. **Exception Frequency**: Count of specific exception types
4. **User Activity**: Active users and operations
5. **System Health**: Memory usage, CPU, thread count

### Recommended Alerts

1. **High Error Rate**: >5% errors in 5-minute window
2. **Slow Responses**: P95 response time >2 seconds
3. **Critical Exceptions**: Any fatal-level log entries
4. **Missing Correlation IDs**: Requests without correlation tracking
5. **Log Volume**: Sudden increases in log volume

## Integration with Existing Systems

### Health Checks

The logging system integrates with health checks to provide operational visibility:

```csharp
public class LoggingHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Check if logs are being written successfully
        // Check disk space for log files
        // Verify Application Insights connectivity
    }
}
```

### Performance Monitoring

Logging integrates with the performance monitoring system to provide comprehensive observability:

- **Request/Response correlation** with performance metrics
- **Exception tracking** with performance impact analysis
- **User journey tracking** across multiple requests

## Troubleshooting

### Common Issues

1. **Missing Correlation IDs**
   - Ensure `CorrelationIdMiddleware` is registered early in pipeline
   - Check that `LogContext.PushProperty` is used correctly

2. **Log File Permissions**
   - Ensure application has write permissions to log directory
   - Check disk space availability

3. **Application Insights Connection**
   - Verify connection string configuration
   - Check network connectivity to Azure

4. **Performance Impact**
   - Monitor log volume and adjust levels if necessary
   - Use async logging for high-throughput scenarios

### Log File Locations

- **Development**: `logs/dev/kchief-dev-YYYYMMDD.log`
- **Production**: `/app/logs/kchief-YYYYMMDD.log`
- **Structured JSON**: `logs/kchief-structured-YYYYMMDD.json`

This comprehensive logging implementation provides the foundation for production-grade observability, enabling effective monitoring, debugging, and operational insights for the K-Chief Marine Automation Platform.
