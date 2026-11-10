namespace Resolve.Api.LearningItems.Create;

public sealed record CreateLearningItemRequest
{
    public required string Url { get; init; }
}
