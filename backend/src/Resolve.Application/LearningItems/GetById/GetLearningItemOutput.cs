using Resolve.Domain;

namespace Resolve.Application.LearningItems.GetById;

public sealed record GetLearningItemOutput(
    int Id,
    ItemType ItemType,
    string Title,
    string? Content,
    DateTime? ArchivedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
