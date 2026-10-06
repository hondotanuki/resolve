namespace Resolve.Api;

public sealed record CreateLearningItemRequest
{
    public required string ItemType { get; init; }
    public required string Title { get; init; }
    public string? Content { get; init; }
}
