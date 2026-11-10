using Resolve.Api.Contracts;
using Resolve.Domain;

namespace Resolve.Api.Mapping;

public static class StudyResultMapping
{
    public static bool TryParse(string value, out StudyResult studyResult)
    {
        switch (value)
        {
            case ApiValues.StudyResults.Success:
                studyResult = StudyResult.Success;
                return true;
            case ApiValues.StudyResults.Failure:
                studyResult = StudyResult.Failure;
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
            StudyResult.Success => ApiValues.StudyResults.Success,
            StudyResult.Failure => ApiValues.StudyResults.Failure,
            _ => throw new ArgumentOutOfRangeException(
                nameof(studyResult),
                studyResult,
                "対応していない学習結果です。"
            ),
        };
    }
}
