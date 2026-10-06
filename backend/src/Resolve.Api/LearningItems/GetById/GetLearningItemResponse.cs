namespace Resolve.Api;

public sealed record GetLearningItemResponse
{
    public int Id { get; init; }
    public required string ItemType { get; init; }
    public required string Title { get; init; }
    public string? Content { get; init; }
    public DateTime? ArchivedAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
};
