using System.ComponentModel.DataAnnotations;
using Resolve.Application.LearningItems.Update;

namespace Resolve.Api.LearningItems.Update;

public sealed record UpdateLearningItemTitleRequest
{
    [MinLength(1)]
    public required string Title { get; init; }
}

public static class UpdateLearningItemTitleEndpoint
{
    public static void MapUpdateLearningItemTitle(this IEndpointRouteBuilder group)
    {
        group
            .MapPatch(
                "/{id:int}",
                async (
                    int id,
                    UpdateLearningItemTitleRequest request,
                    UpdateLearningItemTitleUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    try
                    {
                        return await useCase.ExecuteAsync(id, request.Title, cancellationToken)
                            ? Results.NoContent()
                            : Results.NotFound();
                    }
                    catch (ArgumentException)
                    {
                        return Results.BadRequest();
                    }
                }
            )
            .WithName("UpdateLearningItemTitle")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}
