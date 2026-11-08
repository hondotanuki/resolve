using Resolve.Domain;

namespace Resolve.Application.LearningItems.GetById;

public sealed record GetLearningItemOutput(
    int Id,
    string Title,
    ItemStatus Status,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex
);
