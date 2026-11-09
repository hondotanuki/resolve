using Resolve.Application.LearningItems.Update;

namespace Resolve.Api.LearningItems.Update;

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
                        var output = await useCase.ExecuteAsync(
                            new UpdateLearningItemTitleInput(id, request.Title),
                            cancellationToken
                        );
                        return output.Updated ? Results.NoContent() : Results.NotFound();
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
