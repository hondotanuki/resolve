namespace Resolve.Api.LearningItems.List;

public sealed record ListLearningItemsResponse
{
    public int Id { get; init; }
    public required string ItemType { get; init; }
    public required string Title { get; init; }
    public DateTime? ArchivedAt { get; init; }
}
