using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KChief.Platform.API.Filters;

/// <summary>
/// Action filter that handles model validation errors and returns standardized responses.
/// </summary>
public class ModelValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? 
                               Guid.NewGuid().ToString("N")[..8];

            var validationErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var problemDetails = new ValidationProblemDetails(validationErrors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Model Validation Failed",
                Status = 400,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;
            problemDetails.Extensions["method"] = context.HttpContext.Request.Method;
            problemDetails.Extensions["path"] = context.HttpContext.Request.Path.Value;

            // Add correlation ID to response headers
            context.HttpContext.Response.Headers.Append("X-Correlation-ID", correlationId);

            context.Result = new BadRequestObjectResult(problemDetails);
        }

        base.OnActionExecuting(context);
    }
}
