using Resolve.Api.Mapping;
using Resolve.Application.LearningItems.List;

namespace Resolve.Api.LearningItems.List;

public static class ListLearningItemsEndpoint
{
    public static void MapListLearningItems(this RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/",
                async (ListLearningItemsUseCase useCase, CancellationToken cancellationToken) =>
                {
                    var output = await useCase.ExecuteAsync(cancellationToken);

                    var response = output
                        .Select(x => new ListLearningItemsResponse
                        {
                            Id = x.Id,
                            Title = x.Title,
                            ArchivedAt = x.ArchivedAt,
                            Source = x.Source.ToString(),
                            Category = x.Category.ToString(),
                            ProblemIndex = x.ProblemIndex.ToString(),
                        })
                        .ToList();

                    return Results.Ok(response);
                }
            )
            .WithName("ListLearningItems")
            .Produces<List<ListLearningItemsResponse>>(StatusCodes.Status200OK);
    }
}
