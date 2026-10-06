using Resolve.Api.Mapping;
using Resolve.Application.LearningItems.Create;

namespace Resolve.Api.LearningItems.Create;

public static class CreateLearningItemEndpoint
{
    public static void MapCreateLearningItem(this RouteGroupBuilder group)
    {
        group.MapPost(
            "/",
            async (
                CreateLearningItemRequest request,
                CreateLearningItemUseCase useCase,
                CancellationToken cancellationToken
            ) =>
            {
                if (!ItemTypeMapping.TryParse(request.ItemType, out var itemType))
                {
                    return Results.BadRequest();
                }

                var input = new CreateLearningItemInput
                {
                    ItemType = itemType,
                    Title = request.Title,
                    Content = request.Content,
                };
                try
                {
                    var output = await useCase.ExecuteAsync(input, cancellationToken);

                    var response = new CreateLearningItemResponse
                    {
                        Id = output.Id,
                        ItemType = ItemTypeMapping.ToApiValue(output.ItemType),
                        Title = output.Title,
                        ArchivedAt = output.ArchivedAt,
                    };

                    return Results.Created($"/learning-items/{response.Id}", response);
                }
                catch (ArgumentException)
                {
                    return Results.BadRequest();
                }
            }
        );
    }
}
