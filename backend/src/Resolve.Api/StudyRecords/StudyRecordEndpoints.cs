using Resolve.Api.StudyRecords.Create;
using Resolve.Api.StudyRecords.List;

namespace Resolve.Api.StudyRecords;

public static class StudyRecordEndpoints
{
    public static void MapStudyRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/learning-items");

        group.MapCreateStudyRecord();
        group.MapListStudyRecords();
    }
}
