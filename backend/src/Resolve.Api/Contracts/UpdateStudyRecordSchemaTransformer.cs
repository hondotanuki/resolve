using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Resolve.Api.StudyRecords.Update;

namespace Resolve.Api.Contracts;

public sealed class UpdateStudyRecordSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.JsonTypeInfo.Type != typeof(UpdateStudyRecordRequest))
        {
            return Task.CompletedTask;
        }

        schema.AnyOf = new List<IOpenApiSchema>
        {
            new OpenApiSchema { Required = new HashSet<string> { "result" } },
            new OpenApiSchema { Required = new HashSet<string> { "studiedAt" } },
            new OpenApiSchema { Required = new HashSet<string> { "durationSeconds" } },
            new OpenApiSchema { Required = new HashSet<string> { "memo" } },
        };

        return Task.CompletedTask;
    }
}
