using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HMI.Platform.API.Filters;

/// <summary>
/// Exception filter that handles OperationCanceledException specifically.
/// </summary>
public class OperationCancelledExceptionFilter : IExceptionFilter
{
    private readonly ILogger<OperationCancelledExceptionFilter> _logger;

    public OperationCancelledExceptionFilter(ILogger<OperationCancelledExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException)
        {
            var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? 
                               Guid.NewGuid().ToString("N")[..8];

            _logger.LogInformation("Request was cancelled. CorrelationId: {CorrelationId}, Path: {Path}",
                correlationId, context.HttpContext.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Title = "Request Cancelled",
                Status = 499, // Client Closed Request
                Detail = "The request was cancelled by the client.",
                Instance = context.HttpContext.Request.Path
            };

            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
            problemDetails.Extensions["method"] = context.HttpContext.Request.Method;
            problemDetails.Extensions["path"] = context.HttpContext.Request.Path.Value;

            context.HttpContext.Response.Headers.Append("X-Correlation-ID", correlationId);
            context.Result = new ObjectResult(problemDetails) { StatusCode = 499 };
            context.ExceptionHandled = true;
        }
    }
}
