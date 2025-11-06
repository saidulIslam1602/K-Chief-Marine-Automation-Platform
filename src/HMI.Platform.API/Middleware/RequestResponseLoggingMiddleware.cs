using System.Diagnostics;
using System.Text;
using Serilog;
using Serilog.Context;

namespace HMI.Platform.API.Middleware;

/// <summary>
/// Middleware for comprehensive request and response logging with structured data.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next, 
        ILogger<RequestResponseLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString() ?? "unknown";

        // Log request
        await LogRequestAsync(context, correlationId);

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);
            
            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;
        
        // Read request body if it exists and is not too large
        string? requestBody = null;
        if (ShouldLogRequestBody(request))
        {
            requestBody = await ReadRequestBodyAsync(request);
        }

        // Get client information
        var clientIp = GetClientIpAddress(context);
        var userAgent = request.Headers.UserAgent.ToString();
        
        using (LogContext.PushProperty("RequestId", correlationId))
        using (LogContext.PushProperty("ClientIP", clientIp))
        using (LogContext.PushProperty("UserAgent", userAgent))
        using (LogContext.PushProperty("RequestPath", request.Path.Value))
        using (LogContext.PushProperty("RequestMethod", request.Method))
        using (LogContext.PushProperty("RequestScheme", request.Scheme))
        using (LogContext.PushProperty("RequestHost", request.Host.Value))
        using (LogContext.PushProperty("RequestQueryString", request.QueryString.Value))
        {
            if (requestBody != null)
            {
                using (LogContext.PushProperty("RequestBody", requestBody))
                {
                    Log.Information("HTTP Request: {Method} {Path} from {ClientIP}",
                        request.Method, request.Path, clientIp);
                }
            }
            else
            {
                Log.Information("HTTP Request: {Method} {Path} from {ClientIP}",
                    request.Method, request.Path, clientIp);
            }
        }
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMs)
    {
        var response = context.Response;
        
        // Read response body if it exists and is not too large
        string? responseBody = null;
        if (ShouldLogResponseBody(response))
        {
            responseBody = await ReadResponseBodyAsync(response);
        }

        var logLevel = DetermineLogLevel(response.StatusCode);
        
        using (LogContext.PushProperty("RequestId", correlationId))
        using (LogContext.PushProperty("ResponseStatusCode", response.StatusCode))
        using (LogContext.PushProperty("ResponseContentType", response.ContentType))
        using (LogContext.PushProperty("ResponseContentLength", response.ContentLength))
        using (LogContext.PushProperty("ElapsedMilliseconds", elapsedMs))
        {
            if (responseBody != null)
            {
                using (LogContext.PushProperty("ResponseBody", responseBody))
                {
                    Log.Write(logLevel, "HTTP Response: {StatusCode} in {ElapsedMs}ms",
                        response.StatusCode, elapsedMs);
                }
            }
            else
            {
                Log.Write(logLevel, "HTTP Response: {StatusCode} in {ElapsedMs}ms",
                    response.StatusCode, elapsedMs);
            }
        }
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        // Only log request body for specific content types and reasonable sizes
        if (request.ContentLength == null || request.ContentLength > 10240) // 10KB limit
            return false;

        var contentType = request.ContentType?.ToLowerInvariant();
        return contentType != null && (
            contentType.Contains("application/json") ||
            contentType.Contains("application/xml") ||
            contentType.Contains("text/plain") ||
            contentType.Contains("application/x-www-form-urlencoded"));
    }

    private static bool ShouldLogResponseBody(HttpResponse response)
    {
        // Only log response body for specific content types and reasonable sizes
        if (response.ContentLength == null || response.ContentLength > 10240) // 10KB limit
            return false;

        var contentType = response.ContentType?.ToLowerInvariant();
        return contentType != null && (
            contentType.Contains("application/json") ||
            contentType.Contains("application/xml") ||
            contentType.Contains("text/plain"));
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();
        
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        
        using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        
        return body;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP addresses (load balancers, proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static Serilog.Events.LogEventLevel DetermineLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => Serilog.Events.LogEventLevel.Error,
            >= 400 => Serilog.Events.LogEventLevel.Warning,
            >= 300 => Serilog.Events.LogEventLevel.Information,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}
