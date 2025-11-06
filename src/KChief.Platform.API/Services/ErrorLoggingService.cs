using System.Text.Json;
using KChief.Platform.Core.Exceptions;

namespace KChief.Platform.API.Services;

/// <summary>
/// Service for comprehensive error logging with structured data and correlation tracking.
/// </summary>
public class ErrorLoggingService
{
    private readonly ILogger<ErrorLoggingService> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorLoggingService(ILogger<ErrorLoggingService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Logs an exception with comprehensive context information.
    /// </summary>
    public void LogException(Exception exception, HttpContext? httpContext = null, string? correlationId = null)
    {
        correlationId ??= httpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N")[..8];

        // Create comprehensive log data
        var logData = new Dictionary<string, object>
        {
            ["correlationId"] = correlationId,
            ["exception"] = new Dictionary<string, object?>
            {
                ["type"] = exception.GetType().Name,
                ["message"] = exception.Message,
                ["stackTrace"] = exception.StackTrace,
                ["source"] = exception.Source,
                ["hResult"] = exception.HResult,
                ["errorCode"] = exception is KChiefException kchiefEx ? kchiefEx.ErrorCode : null,
                ["context"] = exception is KChiefException kchiefEx2 ? kchiefEx2.Context : null,
                ["innerException"] = exception.InnerException != null ? new Dictionary<string, object?>
                {
                    ["type"] = exception.InnerException.GetType().Name,
                    ["message"] = exception.InnerException.Message,
                    ["stackTrace"] = exception.InnerException.StackTrace
                } : null
            },
            ["request"] = httpContext != null ? new Dictionary<string, object?>
            {
                ["method"] = httpContext.Request.Method,
                ["path"] = httpContext.Request.Path.Value,
                ["queryString"] = httpContext.Request.QueryString.Value,
                ["headers"] = GetSafeHeaders(httpContext.Request.Headers),
                ["userAgent"] = httpContext.Request.Headers.UserAgent.ToString(),
                ["remoteIpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString(),
                ["contentType"] = httpContext.Request.ContentType
            } : null,
            ["user"] = httpContext?.User?.Identity?.Name,
            ["timestamp"] = DateTimeOffset.UtcNow,
            ["environment"] = _environment.EnvironmentName
        };

        var logLevel = DetermineLogLevel(exception);
        var message = "Exception occurred: {ExceptionType} - {ExceptionMessage} [CorrelationId: {CorrelationId}]";

        _logger.Log(logLevel, exception, message, 
            exception.GetType().Name, 
            exception.Message, 
            correlationId);

        // Log structured data as separate entry for better searchability
        _logger.LogInformation("Exception Details: {ExceptionDetails}", 
            JsonSerializer.Serialize(logData, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }

    /// <summary>
    /// Logs a business operation error with context.
    /// </summary>
    public void LogBusinessError(string operation, string errorMessage, object? context = null, string? correlationId = null)
    {
        correlationId ??= Guid.NewGuid().ToString("N")[..8];

        var logData = new
        {
            CorrelationId = correlationId,
            Operation = operation,
            ErrorMessage = errorMessage,
            Context = context,
            Timestamp = DateTimeOffset.UtcNow,
            Environment = _environment.EnvironmentName
        };

        _logger.LogWarning("Business operation error: {Operation} - {ErrorMessage} [CorrelationId: {CorrelationId}]",
            operation, errorMessage, correlationId);

        _logger.LogInformation("Business Error Details: {BusinessErrorDetails}",
            JsonSerializer.Serialize(logData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }

    /// <summary>
    /// Logs a security-related event.
    /// </summary>
    public void LogSecurityEvent(string eventType, string description, HttpContext? httpContext = null, string? correlationId = null)
    {
        correlationId ??= httpContext?.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N")[..8];

        var logData = new
        {
            CorrelationId = correlationId,
            EventType = eventType,
            Description = description,
            Request = httpContext != null ? new
            {
                Method = httpContext.Request.Method,
                Path = httpContext.Request.Path.Value,
                RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers.UserAgent.ToString()
            } : null,
            User = httpContext?.User?.Identity?.Name,
            Timestamp = DateTimeOffset.UtcNow,
            Environment = _environment.EnvironmentName
        };

        _logger.LogWarning("Security event: {EventType} - {Description} [CorrelationId: {CorrelationId}]",
            eventType, description, correlationId);

        _logger.LogInformation("Security Event Details: {SecurityEventDetails}",
            JsonSerializer.Serialize(logData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }

    private static LogLevel DetermineLogLevel(Exception exception)
    {
        return exception switch
        {
            VesselNotFoundException or EngineNotFoundException => LogLevel.Information,
            ValidationException or ArgumentException or ArgumentNullException => LogLevel.Warning,
            VesselOperationException or ProtocolException => LogLevel.Warning,
            UnauthorizedAccessException => LogLevel.Warning,
            TimeoutException => LogLevel.Warning,
            OperationCanceledException => LogLevel.Information,
            _ => LogLevel.Error
        };
    }

    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        var safeHeaders = new Dictionary<string, string>();
        var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Set-Cookie", "X-API-Key", "X-Auth-Token"
        };

        foreach (var header in headers)
        {
            if (!sensitiveHeaders.Contains(header.Key))
            {
                safeHeaders[header.Key] = string.Join(", ", header.Value.AsEnumerable());
            }
            else
            {
                safeHeaders[header.Key] = "[REDACTED]";
            }
        }

        return safeHeaders;
    }
}
