using Resolve.Domain;

namespace Resolve.Infrastructure.External;

public sealed record OmcProblemInfo(
    Uri ProblemUrl,
    string Title,
    string ContestId,
    string ContestName,
    DateOnly ContestStartDate,
    ContestCategory Category,
    char ProblemIndex
);
