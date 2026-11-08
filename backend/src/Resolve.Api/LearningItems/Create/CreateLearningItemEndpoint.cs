using Resolve.Api.Mapping;
using Resolve.Application.LearningItems.Create;
using Resolve.Domain;

namespace Resolve.Api.LearningItems.Create;

public static class CreateLearningItemEndpoint
{
    public static void MapCreateLearningItem(this RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async (
                    CreateLearningItemRequest request,
                    CreateLearningItemUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (
                        !Enum.TryParse<ProblemSource>(request.Source, false, out var source)
                        || !Enum.IsDefined(source)
                        || source.ToString() != request.Source
                        || !Enum.TryParse<ContestCategory>(
                            request.Category,
                            false,
                            out var category
                        )
                        || !Enum.IsDefined(category)
                        || category.ToString() != request.Category
                        || request.ProblemIndex is not { Length: 1 }
                    )
                        return Results.BadRequest();

                    var input = new CreateLearningItemInput
                    {
                        Title = request.Title,
                        Source = source,
                        Category = category,
                        ProblemIndex = request.ProblemIndex[0],
                    };
                    try
                    {
                        var output = await useCase.ExecuteAsync(input, cancellationToken);

                        var response = new CreateLearningItemResponse
                        {
                            Id = output.Id,
                            Title = output.Title,
                            ArchivedAt = output.ArchivedAt,
                            Source = output.Source.ToString(),
                            Category = output.Category.ToString(),
                            ProblemIndex = output.ProblemIndex.ToString(),
                        };

                        return Results.Created($"/learning-items/{response.Id}", response);
                    }
                    catch (ArgumentException)
                    {
                        return Results.BadRequest();
                    }
                }
            )
            .WithName("CreateLearningItem")
            .Produces<CreateLearningItemResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
