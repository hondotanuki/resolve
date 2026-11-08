using Resolve.Api.Contracts;

namespace Resolve.Api.LearningItems.List;

public sealed record ListLearningItemsResponse
{
    public required int Id { get; init; }

    public required string Title { get; init; }
    public required DateTime? ArchivedAt { get; init; }

    [ApiAllowedValues(ApiValueSet.ProblemSources)]
    public required string Source { get; init; }

    [ApiAllowedValues(ApiValueSet.ContestCategories)]
    public required string Category { get; init; }
    public required string ProblemIndex { get; init; }
}
