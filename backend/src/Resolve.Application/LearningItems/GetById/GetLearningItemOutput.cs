using Resolve.Domain;

namespace Resolve.Application.LearningItems.GetById;

public sealed record GetLearningItemOutput(
    int Id,
    ItemType ItemType,
    string Title,
    ItemStatus Status,
    string? Content,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex
);
