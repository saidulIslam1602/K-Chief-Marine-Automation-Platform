using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using KChief.Platform.Core.Exceptions;

namespace KChief.Platform.API.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and converts them to standardized error responses.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N")[..8];
        
        _logger.LogError(exception, 
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId, context.Request.Path, context.Request.Method);

        var problemDetails = CreateProblemDetails(context, exception, correlationId);
        
        var response = context.Response;
        response.ContentType = "application/problem+json";
        response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        // Add correlation ID to response headers
        if (!response.Headers.ContainsKey("X-Correlation-ID"))
        {
            response.Headers.Append("X-Correlation-ID", correlationId);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string correlationId)
    {
        var problemDetails = exception switch
        {
            VesselNotFoundException vesselNotFound => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Vessel Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = vesselNotFound.Message,
                Instance = context.Request.Path
            },

            EngineNotFoundException engineNotFound => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Engine Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = engineNotFound.Message,
                Instance = context.Request.Path
            },

            ValidationException validationEx => new ValidationProblemDetails(validationEx.ValidationErrors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = validationEx.Message,
                Instance = context.Request.Path
            },

            VesselOperationException operationEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Vessel Operation Failed",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = operationEx.Message,
                Instance = context.Request.Path
            },

            ProtocolException protocolEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.2",
                Title = "Protocol Error",
                Status = (int)HttpStatusCode.BadGateway,
                Detail = protocolEx.Message,
                Instance = context.Request.Path
            },

            ArgumentException argEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid Argument",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = argEx.Message,
                Instance = context.Request.Path
            },

            UnauthorizedAccessException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "Access denied. Authentication required.",
                Instance = context.Request.Path
            },

            TimeoutException timeoutEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5",
                Title = "Request Timeout",
                Status = (int)HttpStatusCode.RequestTimeout,
                Detail = timeoutEx.Message,
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path
            }
        };

        // Add correlation ID
        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        // Add error code for custom exceptions
        if (exception is KChiefException kchiefEx)
        {
            problemDetails.Extensions["errorCode"] = kchiefEx.ErrorCode;
            
            // Add context data if available
            if (kchiefEx.Context.Count > 0)
            {
                problemDetails.Extensions["context"] = kchiefEx.Context;
            }
        }

        // Add stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    message = exception.InnerException.Message,
                    stackTrace = exception.InnerException.StackTrace
                };
            }
        }

        // Add request information
        problemDetails.Extensions["method"] = context.Request.Method;
        problemDetails.Extensions["path"] = context.Request.Path.Value;
        
        if (context.Request.QueryString.HasValue)
        {
            problemDetails.Extensions["queryString"] = context.Request.QueryString.Value;
        }

        return problemDetails;
    }
}
