using Resolve.Domain;

namespace Resolve.Application.LearningItems.Create;

public sealed record CreateLearningItemInput
{
    public required string Title { get; init; }
    public required ProblemSource Source { get; init; }
    public required ContestCategory Category { get; init; }
    public required char ProblemIndex { get; init; }
}
