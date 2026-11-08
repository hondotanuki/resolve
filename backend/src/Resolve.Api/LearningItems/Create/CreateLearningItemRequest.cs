using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Resolve.Api.Contracts;

namespace Resolve.Api.LearningItems.Create;

public sealed record CreateLearningItemRequest
{
    [ApiAllowedValues(ApiValueSet.ItemTypes)]
    public required string ItemType { get; init; }

    [MinLength(1)]
    [Description("空文字または空白文字のみのタイトルは指定できません。")]
    public required string Title { get; init; }
    public string? Content { get; init; }

    [ApiAllowedValues(ApiValueSet.ProblemSources)]
    public required string Source { get; init; }

    [ApiAllowedValues(ApiValueSet.ContestCategories)]
    public required string Category { get; init; }

    [RegularExpression("^[A-Z]$")]
    public required string ProblemIndex { get; init; }
}
