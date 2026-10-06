using Resolve.Domain;

namespace Resolve.Application;

public sealed record ListLearningItemsOutput(
    int Id,
    ItemType ItemType,
    string Title,
    DateTime? ArchivedAt
);
