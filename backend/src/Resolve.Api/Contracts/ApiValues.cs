namespace Resolve.Api.Contracts;

public static class ApiValues
{
    public static IReadOnlyList<string> GetAllowed(ApiValueSet valueSet) =>
        valueSet switch
        {
            ApiValueSet.StudyResults => [StudyResults.Success, StudyResults.Failure],
            ApiValueSet.ItemStatuses =>
            [
                ItemStatuses.Unstarted,
                ItemStatuses.NeedsReview,
                ItemStatuses.Completed,
            ],
            ApiValueSet.ProblemSources => ["AtCoder", "OnlineMathContest"],
            ApiValueSet.ContestCategories =>
            [
                "Beginner",
                "Regular",
                "RegularPlus",
                "Grand",
                "Expert",
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(valueSet)),
        };

    public static class StudyResults
    {
        public const string Success = "success";
        public const string Failure = "failure";
    }

    public static class ItemStatuses
    {
        public const string Unstarted = "unstarted";
        public const string NeedsReview = "needs_review";
        public const string Completed = "completed";
    }
}
