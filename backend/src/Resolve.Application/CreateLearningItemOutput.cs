using Resolve.Domain;

namespace Resolve.Application;

public sealed record CreateLearningItemOutput(
    int Id,
    ItemType ItemType,
    string Title,
    string? Content,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
