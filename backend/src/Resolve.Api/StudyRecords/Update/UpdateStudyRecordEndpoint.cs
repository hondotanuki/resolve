using Resolve.Api.Mapping;
using Resolve.Application.StudyRecords;
using Resolve.Application.StudyRecords.Update;
using Resolve.Domain;

namespace Resolve.Api.StudyRecords.Update;

public static class UpdateStudyRecordEndpoint
{
    public static void MapUpdateStudyRecord(this RouteGroupBuilder group)
    {
        group
            .MapPatch(
                "/{studyRecordId:int}",
                async (
                    int studyRecordId,
                    UpdateStudyRecordRequest request,
                    UpdateStudyRecordUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (
                        !request.ResultIsSpecified
                        && !request.StudiedAtIsSpecified
                        && !request.DurationSecondsIsSpecified
                        && !request.MemoIsSpecified
                    )
                    {
                        return Results.BadRequest();
                    }

                    OptionalValue<StudyResult> result = default;
                    if (request.ResultIsSpecified)
                    {
                        if (
                            request.Result is null
                            || !StudyResultMapping.TryParse(request.Result, out var value)
                        )
                        {
                            return Results.BadRequest();
                        }
                        result = OptionalValue.Of(value);
                    }

                    OptionalValue<DateTime> studiedAt = default;
                    if (request.StudiedAtIsSpecified)
                    {
                        if (request.StudiedAt.Kind != DateTimeKind.Utc)
                        {
                            return Results.BadRequest();
                        }
                        studiedAt = OptionalValue.Of(request.StudiedAt);
                    }

                    var input = new UpdateStudyRecordInput
                    {
                        Id = studyRecordId,
                        Result = result,
                        StudiedAt = studiedAt,
                        DurationSeconds = request.DurationSecondsIsSpecified
                            ? OptionalValue.Of(request.DurationSeconds)
                            : default,
                        Memo = request.MemoIsSpecified ? OptionalValue.Of(request.Memo) : default,
                    };

                    try
                    {
                        var output = await useCase.ExecuteAsync(input, cancellationToken);

                        if (output is null)
                        {
                            return Results.NotFound();
                        }

                        var response = new UpdateStudyRecordResponse
                        {
                            Id = output.Id,
                            LearningItemId = output.LearningItemId,
                            Result = StudyResultMapping.ToApiValue(output.Result),
                            StudiedAt = output.StudiedAt,
                            DurationSeconds = output.DurationSeconds,
                            TargetSeconds = output.TargetSeconds,
                            Memo = output.Memo,
                            CreatedAt = output.CreatedAt,
                            UpdatedAt = output.UpdatedAt,
                        };
                        return Results.Ok(response);
                    }
                    catch (ArgumentException)
                    {
                        return Results.BadRequest();
                    }
                }
            )
            .WithName("UpdateStudyRecord")
            .WithDescription(
                "更新項目を1つ以上指定してください。APIは項目名の大文字・小文字を区別し、未知の項目は無視します。"
            )
            .Produces<UpdateStudyRecordResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}
