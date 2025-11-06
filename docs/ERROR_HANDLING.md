# Error Handling and Exception Management

## Overview

The K-Chief Marine Automation Platform implements comprehensive error handling and exception management following industry best practices. The system provides standardized error responses, detailed logging with correlation tracking, and graceful error recovery mechanisms.

## Architecture

### Exception Hierarchy

```
Exception
└── KChiefException (Base for all platform exceptions)
    ├── VesselNotFoundException
    ├── EngineNotFoundException
    ├── VesselOperationException
    ├── ProtocolException
    └── ValidationException
```

### Components

1. **Global Exception Middleware**: Catches all unhandled exceptions
2. **Custom Exception Types**: Domain-specific exceptions with context
3. **Exception Filters**: Specialized handling for specific scenarios
4. **Error Logging Service**: Comprehensive logging with correlation IDs
5. **Problem Details**: RFC 7807 compliant error responses

## Custom Exception Types

### KChiefException (Base Class)

Base exception class for all platform-specific exceptions.

```csharp
public abstract class KChiefException : Exception
{
    public abstract string ErrorCode { get; }
    public Dictionary<string, object> Context { get; }
    
    public KChiefException WithContext(string key, object value);
    public KChiefException WithContext(Dictionary<string, object> contextData);
}
```

**Features:**
- Abstract error code for categorization
- Context dictionary for additional data
- Fluent API for adding context
- Serialization support

### VesselNotFoundException

Thrown when a requested vessel is not found.

```csharp
throw new VesselNotFoundException("vessel-001");
```

**Properties:**
- `ErrorCode`: "VESSEL_NOT_FOUND"
- `VesselId`: The ID of the vessel that was not found

### EngineNotFoundException

Thrown when a requested engine is not found.

```csharp
throw new EngineNotFoundException("engine-001", "vessel-001");
```

**Properties:**
- `ErrorCode`: "ENGINE_NOT_FOUND"
- `EngineId`: The ID of the engine that was not found
- `VesselId`: The ID of the vessel (optional)

### VesselOperationException

Thrown when a vessel operation fails.

```csharp
throw new VesselOperationException("vessel-001", "start", "Engine temperature too high")
    .WithContext("Temperature", 95.5)
    .WithContext("MaxTemperature", 85.0);
```

**Properties:**
- `ErrorCode`: "VESSEL_OPERATION_FAILED"
- `VesselId`: The ID of the vessel
- `Operation`: The operation that failed

### ProtocolException

Thrown when a protocol operation fails (OPC UA, Modbus, etc.).

```csharp
throw new ProtocolException("OPC UA", "opc.tcp://localhost:4840", "Connection timeout");
```

**Properties:**
- `ErrorCode`: "PROTOCOL_ERROR"
- `Protocol`: The protocol name
- `Endpoint`: The endpoint (optional)

### ValidationException

Thrown when validation fails.

```csharp
var errors = new Dictionary<string, string[]>
{
    ["name"] = new[] { "Name is required." },
    ["age"] = new[] { "Age must be between 0 and 150." }
};
throw new ValidationException(errors);
```

**Properties:**
- `ErrorCode`: "VALIDATION_ERROR"
- `ValidationErrors`: Dictionary of field errors

## Global Exception Handling Middleware

The `GlobalExceptionHandlingMiddleware` provides centralized exception handling for the entire application.

### Features

- **Automatic Exception Catching**: Catches all unhandled exceptions
- **Standardized Responses**: Converts exceptions to Problem Details format
- **Correlation Tracking**: Adds correlation IDs to all error responses
- **Environment-Aware**: Shows detailed errors in development, sanitized in production
- **Comprehensive Logging**: Logs all exceptions with context

### Response Format

All error responses follow the RFC 7807 Problem Details standard:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Vessel Not Found",
  "status": 404,
  "detail": "Vessel with ID 'vessel-001' was not found.",
  "instance": "/api/vessels/vessel-001",
  "correlationId": "a1b2c3d4",
  "timestamp": "2023-12-07T10:30:00Z",
  "errorCode": "VESSEL_NOT_FOUND",
  "context": {
    "vesselId": "vessel-001"
  },
  "method": "GET",
  "path": "/api/vessels/vessel-001"
}
```

### HTTP Status Code Mapping

| Exception Type | HTTP Status | Description |
|---|---|---|
| `VesselNotFoundException` | 404 | Not Found |
| `EngineNotFoundException` | 404 | Not Found |
| `ValidationException` | 400 | Bad Request |
| `VesselOperationException` | 400 | Bad Request |
| `ProtocolException` | 502 | Bad Gateway |
| `ArgumentException` | 400 | Bad Request |
| `ArgumentNullException` | 400 | Bad Request |
| `UnauthorizedAccessException` | 401 | Unauthorized |
| `TimeoutException` | 408 | Request Timeout |
| `OperationCanceledException` | 499 | Client Closed Request |
| Generic `Exception` | 500 | Internal Server Error |

## Exception Filters

### ModelValidationFilter

Handles model validation errors and returns standardized responses.

```csharp
[ApiController]
public class VesselsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateVessel([FromBody] CreateVesselRequest request)
    {
        // Model validation is automatically handled by ModelValidationFilter
        // Invalid models return 400 with detailed validation errors
    }
}
```

### OperationCancelledExceptionFilter

Handles request cancellation scenarios gracefully.

```csharp
// Automatically handles when clients cancel requests
// Returns 499 Client Closed Request status
```

## Error Logging Service

The `ErrorLoggingService` provides comprehensive error logging with structured data.

### Features

- **Structured Logging**: JSON-formatted log entries
- **Correlation Tracking**: Links related log entries
- **Context Preservation**: Captures request and user context
- **Security-Aware**: Redacts sensitive headers
- **Environment-Aware**: Adjusts detail level based on environment

### Usage

```csharp
public class SomeController : ControllerBase
{
    private readonly ErrorLoggingService _errorLogging;

    public SomeController(ErrorLoggingService errorLogging)
    {
        _errorLogging = errorLogging;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessData([FromBody] DataRequest request)
    {
        try
        {
            // Process data
            return Ok();
        }
        catch (Exception ex)
        {
            _errorLogging.LogException(ex, HttpContext);
            throw; // Re-throw to be handled by global middleware
        }
    }

    [HttpPost("business-operation")]
    public async Task<IActionResult> BusinessOperation([FromBody] BusinessRequest request)
    {
        if (request.Amount > 1000)
        {
            _errorLogging.LogBusinessError(
                "ProcessPayment",
                "Amount exceeds daily limit",
                new { UserId = request.UserId, Amount = request.Amount, Limit = 1000 });
                
            return BadRequest("Amount exceeds daily limit");
        }

        return Ok();
    }
}
```

### Log Levels

| Exception Type | Log Level | Rationale |
|---|---|---|
| `VesselNotFoundException` | Information | Expected business scenario |
| `EngineNotFoundException` | Information | Expected business scenario |
| `ValidationException` | Warning | Client error, needs attention |
| `VesselOperationException` | Warning | Business rule violation |
| `ProtocolException` | Warning | External system issue |
| `UnauthorizedAccessException` | Warning | Security concern |
| `TimeoutException` | Warning | Performance issue |
| `OperationCanceledException` | Information | Normal cancellation |
| Generic exceptions | Error | Unexpected system error |

## Best Practices

### 1. Exception Creation

```csharp
// Good: Specific exception with context
throw new VesselOperationException("vessel-001", "start", "Engine temperature too high")
    .WithContext("Temperature", 95.5)
    .WithContext("MaxTemperature", 85.0);

// Bad: Generic exception without context
throw new Exception("Something went wrong");
```

### 2. Validation

```csharp
// Good: Early validation with specific exceptions
public async Task<bool> SetEngineRPM(string vesselId, string engineId, int rpm)
{
    if (string.IsNullOrWhiteSpace(vesselId))
        throw new ArgumentException("Vessel ID cannot be null or empty.", nameof(vesselId));
        
    if (rpm < 0)
        throw new ArgumentException("RPM cannot be negative.", nameof(rpm));
        
    // Continue with operation
}

// Bad: Late validation with generic returns
public async Task<bool> SetEngineRPM(string vesselId, string engineId, int rpm)
{
    var vessel = await GetVessel(vesselId);
    if (vessel == null || rpm < 0)
        return false; // Caller doesn't know what went wrong
}
```

### 3. Error Context

```csharp
// Good: Rich context information
catch (SqlException ex)
{
    throw new VesselOperationException(vesselId, "update", "Database operation failed")
        .WithContext("SqlErrorNumber", ex.Number)
        .WithContext("SqlState", ex.State)
        .WithContext("TableName", "Vessels");
}

// Bad: Lost context
catch (SqlException ex)
{
    throw new Exception("Database error");
}
```

### 4. Async Exception Handling

```csharp
// Good: Proper async exception handling
public async Task<Vessel> GetVesselAsync(string vesselId)
{
    try
    {
        return await _repository.GetByIdAsync(vesselId);
    }
    catch (EntityNotFoundException)
    {
        throw new VesselNotFoundException(vesselId);
    }
}

// Bad: Blocking async operations
public Vessel GetVessel(string vesselId)
{
    try
    {
        return _repository.GetByIdAsync(vesselId).Result; // Can cause deadlocks
    }
    catch (AggregateException ex)
    {
        // Complex exception unwrapping required
    }
}
```

## Testing Error Handling

### Unit Tests

```csharp
[Test]
public async Task GetVessel_WithInvalidId_ThrowsVesselNotFoundException()
{
    // Arrange
    var service = new VesselControlService();
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<VesselNotFoundException>(
        () => service.GetVesselAsync("invalid-id"));
        
    Assert.That(exception.VesselId, Is.EqualTo("invalid-id"));
    Assert.That(exception.ErrorCode, Is.EqualTo("VESSEL_NOT_FOUND"));
}
```

### Integration Tests

```csharp
[Test]
public async Task GetVessel_WithInvalidId_Returns404WithProblemDetails()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/vessels/invalid-id");
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    
    var content = await response.Content.ReadAsStringAsync();
    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content);
    
    Assert.That(problemDetails.Title, Is.EqualTo("Vessel Not Found"));
    Assert.That(problemDetails.Status, Is.EqualTo(404));
    Assert.That(problemDetails.Extensions["errorCode"], Is.EqualTo("VESSEL_NOT_FOUND"));
}
```

### Error Demo Endpoints

The platform includes demonstration endpoints for testing error handling:

- `GET /api/errordemo/vessel-not-found/{vesselId}` - Vessel not found
- `GET /api/errordemo/engine-not-found/{vesselId}/{engineId}` - Engine not found
- `POST /api/errordemo/validation-error` - Validation errors
- `POST /api/errordemo/operation-error/{vesselId}` - Operation failures
- `GET /api/errordemo/protocol-error` - Protocol errors
- `GET /api/errordemo/generic-error` - Generic system errors
- `GET /api/errordemo/argument-error` - Argument validation
- `GET /api/errordemo/timeout-error` - Timeout scenarios

## Monitoring and Alerting

### Log Analysis

Error logs include structured data for easy analysis:

```json
{
  "correlationId": "a1b2c3d4",
  "exception": {
    "type": "VesselNotFoundException",
    "message": "Vessel with ID 'vessel-001' was not found.",
    "errorCode": "VESSEL_NOT_FOUND",
    "context": {
      "vesselId": "vessel-001"
    }
  },
  "request": {
    "method": "GET",
    "path": "/api/vessels/vessel-001",
    "userAgent": "Mozilla/5.0...",
    "remoteIpAddress": "192.168.1.100"
  },
  "timestamp": "2023-12-07T10:30:00Z"
}
```

### Metrics

Key error metrics to monitor:

- **Error Rate**: Percentage of requests resulting in errors
- **Error Distribution**: Breakdown by exception type
- **Response Time**: Impact of errors on performance
- **Correlation Patterns**: Related errors across requests

### Alerting Rules

Recommended alerting thresholds:

- Error rate > 5% over 5 minutes
- More than 10 500-level errors in 1 minute
- Specific business errors (e.g., payment failures)
- Security events (unauthorized access attempts)

## Production Considerations

### Security

- Sensitive information is never exposed in error responses
- Stack traces are only shown in development environments
- Security events are logged separately with appropriate detail
- Correlation IDs help track issues without exposing system internals

### Performance

- Exception handling has minimal performance impact
- Structured logging is optimized for high throughput
- Error responses are cached where appropriate
- Async patterns prevent blocking on error handling

### Scalability

- Correlation IDs work across distributed systems
- Error context is serializable for message queues
- Log aggregation supports multiple application instances
- Error handling is stateless and thread-safe

This comprehensive error handling system ensures robust, maintainable, and observable error management throughout the K-Chief Marine Automation Platform.
