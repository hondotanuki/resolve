using Resolve.Domain;

namespace Resolve.Application.LearningItems.List;

public sealed record ListLearningItemsOutput(
    int Id,
    ItemType ItemType,
    string Title,
    DateTime? ArchivedAt
);
