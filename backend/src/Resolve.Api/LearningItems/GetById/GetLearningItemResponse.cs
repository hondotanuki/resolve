using Resolve.Api.Contracts;

namespace Resolve.Api.LearningItems.GetById;

public sealed record GetLearningItemResponse
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    [ApiAllowedValues(ApiValueSet.ItemStatuses)]
    public required string Status { get; init; }
    public required DateTime? ArchivedAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    [ApiAllowedValues(ApiValueSet.ProblemSources)]
    public required string Source { get; init; }

    [ApiAllowedValues(ApiValueSet.ContestCategories)]
    public required string Category { get; init; }
    public required string ProblemIndex { get; init; }
    public string? ProblemUrl { get; init; }
    public string? ContestId { get; init; }
    public string? ContestName { get; init; }
    public DateOnly? ContestStartDate { get; init; }
};
