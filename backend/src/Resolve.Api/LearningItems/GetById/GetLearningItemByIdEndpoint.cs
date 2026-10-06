using Resolve.Application;

namespace Resolve.Api;

public static class GetLearningItemByIdEndpoint
{
    public static void MapGetLearningItemById(this RouteGroupBuilder group)
    {
        group.MapGet(
            "/{id:int}",
            async (
                int id,
                GetLearningItemByIdUseCase useCase,
                CancellationToken cancellationToken
            ) =>
            {
                var output = await useCase.ExecuteAsync(id, cancellationToken);

                if (output is null)
                {
                    return Results.NotFound();
                }

                var response = new GetLearningItemResponse
                {
                    Id = output.Id,
                    ItemType = ItemTypeMapping.ToApiValue(output.ItemType),
                    Title = output.Title,
                    Content = output.Content,
                    ArchivedAt = output.ArchivedAt,
                    CreatedAt = output.CreatedAt,
                    UpdatedAt = output.UpdatedAt,
                };

                return Results.Ok(response);
            }
        );
    }
}
