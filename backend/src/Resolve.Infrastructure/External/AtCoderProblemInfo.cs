using Resolve.Domain;

namespace Resolve.Infrastructure.External;

public sealed record AtCoderProblemInfo(
    Uri ProblemUrl,
    string Title,
    string ContestId,
    string ContestName,
    DateOnly ContestStartDate,
    ContestCategory Category,
    char ProblemIndex
);
