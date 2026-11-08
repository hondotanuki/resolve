using Resolve.Application.LearningItems.Create;

namespace Resolve.Api.LearningItems.Create;

public sealed record CreateLearningItemFromUrlRequest
{
    public required string Url { get; init; }
}

public sealed record CreateLearningItemFromUrlResponse(int Id);

public static class CreateLearningItemFromUrlEndpoint
{
    public static void MapCreateLearningItemFromUrl(this IEndpointRouteBuilder group)
    {
        group
            .MapPost(
                "/from-url",
                async (
                    CreateLearningItemFromUrlRequest request,
                    CreateLearningItemFromUrlUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                        return Results.BadRequest();
                    try
                    {
                        var result = await useCase.ExecuteAsync(uri, cancellationToken);
                        var response = new CreateLearningItemFromUrlResponse(result.Id);
                        return result.Created
                            ? Results.Created($"/learning-items/{result.Id}", response)
                            : Results.Ok(response);
                    }
                    catch (ArgumentException)
                    {
                        return Results.BadRequest();
                    }
                    catch (Exception exception)
                        when (exception
                                is HttpRequestException
                                    or InvalidDataException
                                    or InvalidOperationException
                        )
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status502BadGateway,
                            title: "問題情報を取得できませんでした。"
                        );
                    }
                }
            )
            .WithName("CreateLearningItemFromUrl")
            .Produces<CreateLearningItemFromUrlResponse>(StatusCodes.Status201Created)
            .Produces<CreateLearningItemFromUrlResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}
