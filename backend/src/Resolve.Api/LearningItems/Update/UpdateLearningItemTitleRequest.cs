using System.ComponentModel.DataAnnotations;

namespace Resolve.Api.LearningItems.Update;

public sealed record UpdateLearningItemTitleRequest
{
    [MinLength(1)]
    public required string Title { get; init; }
}
