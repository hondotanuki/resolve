using Resolve.Domain;

namespace Resolve.Application.LearningItems.Create;

public sealed record CreateLearningItemOutput(
    int Id,
    string Title,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex
);
