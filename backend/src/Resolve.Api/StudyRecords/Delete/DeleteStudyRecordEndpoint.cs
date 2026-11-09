using Resolve.Application.StudyRecords.Delete;

namespace Resolve.Api.StudyRecords.Delete;

public static class DeleteStudyRecordEndpoint
{
    public static void MapDeleteStudyRecord(this RouteGroupBuilder group)
    {
        group
            .MapDelete(
                "/{studyRecordId:int}",
                async (
                    int studyRecordId,
                    DeleteStudyRecordUseCase useCase,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await useCase.ExecuteAsync(
                        new DeleteStudyRecordInput(studyRecordId),
                        cancellationToken
                    );

                    if (!result.Deleted)
                    {
                        return Results.NotFound();
                    }

                    return Results.NoContent();
                }
            )
            .WithName("DeleteStudyRecord")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
