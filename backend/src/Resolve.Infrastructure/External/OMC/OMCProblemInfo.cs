using Resolve.Domain;

namespace Resolve.Infrastructure.External.OMC;

public sealed record OMCProblemInfo(
    Uri ProblemUrl,
    string Title,
    string ContestId,
    string ContestName,
    DateOnly ContestStartDate,
    ContestCategory Category,
    char ProblemIndex
);
