using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace KChief.Platform.API.Swagger;

/// <summary>
/// Schema filter to add examples to schemas.
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Add examples for common types
        if (context.Type == typeof(string))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString("example");
        }
        else if (context.Type == typeof(int))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiInteger(1);
        }
        else if (context.Type == typeof(double))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiDouble(1.0);
        }
        else if (context.Type == typeof(bool))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiBoolean(true);
        }
        else if (context.Type == typeof(DateTime))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"));
        }
        else if (context.Type.IsEnum)
        {
            var enumValues = Enum.GetValues(context.Type);
            if (enumValues.Length > 0)
            {
                schema.Example = new Microsoft.OpenApi.Any.OpenApiString(enumValues.GetValue(0)!.ToString());
            }
        }
    }
}

