using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Resolve.Api.Contracts;

public sealed class AllowedValuesSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var values = context
            .JsonPropertyInfo?.AttributeProvider?.GetCustomAttributes(
                typeof(ApiAllowedValuesAttribute),
                inherit: true
            )
            .OfType<ApiAllowedValuesAttribute>()
            .SingleOrDefault();

        if (values is not null)
        {
            schema.Enum = ApiValues
                .GetAllowed(values.ValueSet)
                .Select(value => (JsonNode)JsonValue.Create(value)!)
                .ToList();
        }
        return Task.CompletedTask;
    }
}
