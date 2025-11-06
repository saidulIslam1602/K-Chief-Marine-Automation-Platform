using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HMI.Platform.API.Swagger;

/// <summary>
/// Operation filter to add error response documentation.
/// </summary>
public class ErrorResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add common error responses if not already present
        AddErrorResponse(operation, "400", "Bad Request", "The request contains invalid data or validation failed.");
        AddErrorResponse(operation, "401", "Unauthorized", "Authentication required. Please provide a valid token or API key.");
        AddErrorResponse(operation, "403", "Forbidden", "You do not have permission to perform this action.");
        AddErrorResponse(operation, "404", "Not Found", "The requested resource was not found.");
        AddErrorResponse(operation, "409", "Conflict", "The request conflicts with the current state of the resource.");
        AddErrorResponse(operation, "422", "Unprocessable Entity", "The request is well-formed but contains semantic errors.");
        AddErrorResponse(operation, "429", "Too Many Requests", "Rate limit exceeded. Please try again later.");
        AddErrorResponse(operation, "500", "Internal Server Error", "An unexpected error occurred on the server.");
        AddErrorResponse(operation, "503", "Service Unavailable", "The service is temporarily unavailable.");
    }

    private void AddErrorResponse(OpenApiOperation operation, string statusCode, string title, string description)
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.Schema,
                                Id = "ProblemDetails"
                            }
                        }
                    }
                }
            });
        }
    }
}

