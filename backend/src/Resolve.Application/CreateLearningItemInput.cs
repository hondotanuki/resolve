using Resolve.Domain;

namespace Resolve.Application;

public sealed record CreateLearningItemInput
{
    public required ItemType ItemType { get; init; }
    public required string Title { get; init; }
    public string? Content { get; init; }
}
