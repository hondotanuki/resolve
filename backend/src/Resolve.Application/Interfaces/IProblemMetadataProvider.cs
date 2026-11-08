using Resolve.Domain;

namespace Resolve.Application.Interfaces;

public sealed record ProblemMetadata(
    Uri ProblemUrl,
    string Title,
    ProblemSource Source,
    ContestCategory Category,
    char ProblemIndex,
    string ContestId,
    string ContestName,
    DateOnly ContestStartDate
);

public interface IProblemMetadataProvider
{
    Uri Normalize(Uri uri);
    Task<ProblemMetadata> GetAsync(Uri uri, CancellationToken cancellationToken = default);
}
