using Resolve.Api.Mapping;
using Resolve.Application.StudyRecords.Create;

namespace Resolve.Api.StudyRecords.Create;

public static class CreateStudyRecordEndpoint
{
    public static void MapCreateStudyRecord(this RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/{learningItemId:int}/study-records",
                async (
                    int learningItemId,
                    CreateStudyRecordRequest request,
                    CreateStudyRecordUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (!StudyResultMapping.TryParse(request.Result, out var result))
                    {
                        return Results.BadRequest();
                    }

                    if (request.StudiedAt.Kind != DateTimeKind.Utc)
                    {
                        return Results.BadRequest();
                    }

                    var input = new CreateStudyRecordInput
                    {
                        LearningItemId = learningItemId,
                        Result = result,
                        StudiedAt = request.StudiedAt,
                        DurationSeconds = request.DurationSeconds,
                        Memo = request.Memo,
                    };

                    try
                    {
                        var output = await useCase.ExecuteAsync(input, cancellationToken);

                        if (output is null)
                        {
                            return Results.NotFound();
                        }

                        var response = new CreateStudyRecordResponse
                        {
                            Id = output.Id,
                            LearningItemId = output.LearningItemId,
                            Result = StudyResultMapping.ToApiValue(output.Result),
                            StudiedAt = output.StudiedAt,
                            DurationSeconds = output.DurationSeconds,
                            Memo = output.Memo,
                            CreatedAt = output.CreatedAt,
                            UpdatedAt = output.UpdatedAt,
                            Status = ItemStatusMapping.ToApiValue(output.Status),
                        };

                        return Results.Created($"/study-records/{output.Id}", response);
                    }
                    catch (ArgumentException)
                    {
                        return Results.BadRequest();
                    }
                }
            )
            .WithName("CreateStudyRecord")
            .Produces<CreateStudyRecordResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}
