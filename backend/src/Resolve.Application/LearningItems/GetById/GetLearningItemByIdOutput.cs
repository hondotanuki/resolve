using Resolve.Domain;

namespace Resolve.Application.LearningItems.GetById;

public sealed record GetLearningItemByIdOutput(
    int Id,
    string Title,
    ItemStatus Status,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex,
    string ProblemUrl,
    string ContestId,
    string ContestName,
    DateOnly ContestStartDate
);
