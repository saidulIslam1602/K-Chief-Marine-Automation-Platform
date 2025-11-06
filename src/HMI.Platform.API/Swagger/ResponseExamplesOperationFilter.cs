using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace HMI.Platform.API.Swagger;

/// <summary>
/// Operation filter to add example responses.
/// </summary>
public class ResponseExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add example for 200 OK responses
        if (operation.Responses.TryGetValue("200", out var okResponse))
        {
            AddExample(okResponse, context, "Success");
        }

        // Add example for 201 Created responses
        if (operation.Responses.TryGetValue("201", out var createdResponse))
        {
            AddExample(createdResponse, context, "Created");
        }

        // Add example for 400 Bad Request responses
        if (operation.Responses.TryGetValue("400", out var badRequestResponse))
        {
            badRequestResponse.Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "The request contains invalid data.",
                        errors = new Dictionary<string, string[]>
                        {
                            ["field1"] = new[] { "Field1 is required", "Field1 must be at least 3 characters" },
                            ["field2"] = new[] { "Field2 must be a valid email address" }
                        }
                    }))
                }
            };
        }

        // Add example for 401 Unauthorized responses
        if (operation.Responses.TryGetValue("401", out var unauthorizedResponse))
        {
            unauthorizedResponse.Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(new
                    {
                        type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                        title = "Unauthorized",
                        status = 401,
                        detail = "Authentication required. Please provide a valid token or API key."
                    }))
                }
            };
        }

        // Add example for 404 Not Found responses
        if (operation.Responses.TryGetValue("404", out var notFoundResponse))
        {
            notFoundResponse.Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "The requested resource was not found."
                    }))
                }
            };
        }

        // Add example for 429 Too Many Requests responses
        if (operation.Responses.TryGetValue("429", out var tooManyRequestsResponse))
        {
            tooManyRequestsResponse.Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(new
                    {
                        type = "https://tools.ietf.org/html/rfc6585#section-4",
                        title = "Too Many Requests",
                        status = 429,
                        detail = "Rate limit exceeded. Please try again later.",
                        retryAfter = 60
                    }))
                }
            };
        }

        // Add example for 500 Internal Server Error responses
        if (operation.Responses.TryGetValue("500", out var serverErrorResponse))
        {
            serverErrorResponse.Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = OpenApiAnyFactory.CreateFromJson(JsonSerializer.Serialize(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                        title = "Internal Server Error",
                        status = 500,
                        detail = "An error occurred while processing your request.",
                        correlationId = "abc123def456"
                    }))
                }
            };
        }
    }

    private void AddExample(OpenApiResponse response, OperationFilterContext context, string exampleName)
    {
        if (response.Content == null || !response.Content.Any())
        {
            return;
        }

        // Try to get example from the response type
        var responseType = context.MethodInfo.ReturnType;
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            responseType = responseType.GetGenericArguments()[0];
        }

        // Add example if we can determine the type
        foreach (var content in response.Content.Values)
        {
            if (content.Example == null && responseType != null)
            {
                // Try to create a simple example based on the type
                var example = CreateExampleForType(responseType);
                if (example != null)
                {
                    content.Example = example;
                }
            }
        }
    }

    private Microsoft.OpenApi.Any.IOpenApiAny? CreateExampleForType(Type type)
    {
        // This is a simplified example generator
        // In a real implementation, you might use reflection to create more complex examples
        if (type == typeof(string))
        {
            return new Microsoft.OpenApi.Any.OpenApiString("example");
        }
        else if (type == typeof(int))
        {
            return new Microsoft.OpenApi.Any.OpenApiInteger(1);
        }
        else if (type == typeof(bool))
        {
            return new Microsoft.OpenApi.Any.OpenApiBoolean(true);
        }

        return null;
    }
}

