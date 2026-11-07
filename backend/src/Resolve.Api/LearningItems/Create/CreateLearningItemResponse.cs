using Resolve.Api.Contracts;

namespace Resolve.Api.LearningItems.Create;

public sealed record CreateLearningItemResponse
{
    public required int Id { get; init; }

    [ApiAllowedValues(ApiValueSet.ItemTypes)]
    public required string ItemType { get; init; }
    public required string Title { get; init; }
    public required DateTime? ArchivedAt { get; init; }
}
