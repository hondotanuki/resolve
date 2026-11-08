using Resolve.Api.Mapping;
using Resolve.Application.StudyRecords.List;

namespace Resolve.Api.StudyRecords.List;

public static class ListStudyRecordsEndpoint
{
    public static void MapListStudyRecords(this RouteGroupBuilder group)
    {
        group
            .MapGet(
                "/{learningItemId:int}/study-records",
                async (
                    int learningItemId,
                    ListStudyRecordsUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    var output = await useCase.ExecuteAsync(learningItemId, cancellationToken);

                    if (output is null)
                    {
                        return Results.NotFound();
                    }

                    var response = output
                        .Select(output => new ListStudyRecordsResponse
                        {
                            Id = output.Id,
                            LearningItemId = output.LearningItemId,
                            Result = StudyResultMapping.ToApiValue(output.Result),
                            StudiedAt = output.StudiedAt,
                            DurationSeconds = output.DurationSeconds,
                            TargetSeconds = output.TargetSeconds,
                            Memo = output.Memo,
                        })
                        .ToList();

                    return Results.Ok(response);
                }
            )
            .WithName("ListStudyRecords")
            .Produces<List<ListStudyRecordsResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
