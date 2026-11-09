using Resolve.Domain;

namespace Resolve.Application.LearningItems.List;

public sealed record ListLearningItemsOutput(
    int Id,
    string Title,
    DateTime? ArchivedAt,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex,
    string ProblemUrl,
    string ContestId,
    string ContestName,
    DateOnly ContestStartDate
);
