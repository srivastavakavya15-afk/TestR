using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestR.Api.Infrastructure;

public sealed class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is null)
        {
            return;
        }

        foreach (var (name, property) in schema.Properties)
        {
            if (!property.Nullable)
            {
                schema.Required.Add(name);
            }
        }
    }
}
