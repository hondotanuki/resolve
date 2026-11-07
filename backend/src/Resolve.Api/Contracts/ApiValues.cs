namespace Resolve.Api.Contracts;

public static class ApiValues
{
    public static IReadOnlyList<string> GetAllowed(ApiValueSet valueSet) =>
        valueSet switch
        {
            ApiValueSet.ItemTypes =>
            [
                ItemTypes.AlgorithmProblem,
                ItemTypes.EnglishWord,
                ItemTypes.EnglishArticle,
            ],
            ApiValueSet.StudyResults =>
            [
                StudyResults.Success,
                StudyResults.Failure,
                StudyResults.Reviewed,
            ],
            ApiValueSet.ItemStatuses =>
            [
                ItemStatuses.Unstarted,
                ItemStatuses.NeedsReview,
                ItemStatuses.Completed,
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(valueSet)),
        };

    public static class ItemTypes
    {
        public const string AlgorithmProblem = "algorithm_problem";
        public const string EnglishWord = "english_word";
        public const string EnglishArticle = "english_article";
    }

    public static class StudyResults
    {
        public const string Success = "success";
        public const string Failure = "failure";
        public const string Reviewed = "reviewed";
    }

    public static class ItemStatuses
    {
        public const string Unstarted = "unstarted";
        public const string NeedsReview = "needs_review";
        public const string Completed = "completed";
    }
}
