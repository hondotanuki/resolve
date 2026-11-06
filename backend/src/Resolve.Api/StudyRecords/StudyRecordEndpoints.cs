using Resolve.Api.StudyRecords.Create;
using Resolve.Api.StudyRecords.Delete;
using Resolve.Api.StudyRecords.List;
using Resolve.Api.StudyRecords.Update;

namespace Resolve.Api.StudyRecords;

public static class StudyRecordEndpoints
{
    public static void MapStudyRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var learningItemGroup = app.MapGroup("/learning-items");

        learningItemGroup.MapCreateStudyRecord();
        learningItemGroup.MapListStudyRecords();

        var studyRecordGroup = app.MapGroup("/study-records");
        studyRecordGroup.MapDeleteStudyRecord();
        studyRecordGroup.MapUpdateStudyRecord();
    }
}
