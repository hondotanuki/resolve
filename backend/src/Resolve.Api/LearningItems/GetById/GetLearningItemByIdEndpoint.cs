using Resolve.Api.Mapping;
using Resolve.Application.LearningItems.GetById;

namespace Resolve.Api.LearningItems.GetById;

public static class GetLearningItemByIdEndpoint
{
    public static void MapGetLearningItemById(this RouteGroupBuilder group)
    {
        group
            .MapGet(
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
                        Title = output.Title,
                        Status = ItemStatusMapping.ToApiValue(output.Status),
                        ArchivedAt = output.ArchivedAt,
                        CreatedAt = output.CreatedAt,
                        UpdatedAt = output.UpdatedAt,
                        Source = output.Source.ToString(),
                        Category = output.Category.ToString(),
                        ProblemIndex = output.ProblemIndex.ToString(),
                    };

                    return Results.Ok(response);
                }
            )
            .WithName("GetLearningItemById")
            .Produces<GetLearningItemResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
