using System.Text.Json;
using Resolve.Application.Interfaces;
using Resolve.Application.LearningItems.Create;

namespace Resolve.Api.LearningItems.Create;

public static class CreateLearningItemEndpoint
{
    public static void MapCreateLearningItem(this IEndpointRouteBuilder group)
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
                    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "対応している問題URLを指定してください。"
                        );
                    try
                    {
                        var result = await useCase.ExecuteAsync(
                            new CreateLearningItemInput(uri),
                            cancellationToken
                        );
                        var response = new CreateLearningItemResponse(result.Id);
                        return result.Created
                            ? Results.Created($"/learning-items/{result.Id}", response)
                            : Results.Ok(response);
                    }
                    catch (UnsupportedProblemUrlException)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status400BadRequest,
                            title: "対応している問題URLを指定してください。"
                        );
                    }
                    catch (UnsupportedProblemCategoryException)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status422UnprocessableEntity,
                            title: "この問題区分には目標時間が設定されていません。"
                        );
                    }
                    catch (OperationCanceledException)
                        when (!cancellationToken.IsCancellationRequested)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status504GatewayTimeout,
                            title: "問題情報の取得がタイムアウトしました。"
                        );
                    }
                    catch (Exception exception)
                        when (exception is HttpRequestException or IOException or JsonException)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status502BadGateway,
                            title: "問題情報を取得できませんでした。"
                        );
                    }
                }
            )
            .WithName("CreateLearningItem")
            .Produces<CreateLearningItemResponse>(StatusCodes.Status201Created)
            .Produces<CreateLearningItemResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status504GatewayTimeout)
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }
}
