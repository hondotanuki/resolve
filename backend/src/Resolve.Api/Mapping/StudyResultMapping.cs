using Resolve.Domain;

namespace Resolve.Api.Mapping;

public static class StudyResultMapping
{
    public static bool TryParse(string value, out StudyResult studyResult)
    {
        switch (value)
        {
            case "success":
                studyResult = StudyResult.Success;
                return true;
            case "failure":
                studyResult = StudyResult.Failure;
                return true;
            case "reviewed":
                studyResult = StudyResult.Reviewed;
                return true;
            default:
                studyResult = default;
                return false;
        }
    }

    public static string ToApiValue(StudyResult studyResult)
    {
        return studyResult switch
        {
            StudyResult.Success => "success",
            StudyResult.Failure => "failure",
            StudyResult.Reviewed => "reviewed",
            _ => throw new ArgumentOutOfRangeException(nameof(studyResult), studyResult, null),
        };
    }
}
