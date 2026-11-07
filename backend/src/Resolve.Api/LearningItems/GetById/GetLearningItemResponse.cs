using Resolve.Api.Contracts;

namespace Resolve.Api.LearningItems.GetById;

public sealed record GetLearningItemResponse
{
    public required int Id { get; init; }

    [ApiAllowedValues(ApiValueSet.ItemTypes)]
    public required string ItemType { get; init; }
    public required string Title { get; init; }

    [ApiAllowedValues(ApiValueSet.ItemStatuses)]
    public required string Status { get; init; }
    public required string? Content { get; init; }
    public required DateTime? ArchivedAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
};
