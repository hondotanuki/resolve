using Resolve.Domain;

namespace Resolve.Application.LearningItems.List;

public sealed record ListLearningItemsOutput(
    int Id,
    string Title,
    DateTime? ArchivedAt,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex,
    string? ProblemUrl = null,
    string? ContestId = null,
    string? ContestName = null,
    DateOnly? ContestStartDate = null
);
