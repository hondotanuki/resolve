namespace Resolve.Api;

public sealed record CreateLearningItemResponse
{
    public int Id { get; init; }
    public required string ItemType { get; init; }
    public required string Title { get; init; }
    public DateTime? ArchivedAt { get; init; }
}
