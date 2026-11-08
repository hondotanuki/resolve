using System.Text.RegularExpressions;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Infrastructure.External;

public sealed class ProblemMetadataProvider(
    AtCoderProblemInfoProvider atCoder,
    OmcProblemInfoProvider omc
) : IProblemMetadataProvider
{
    public Uri Normalize(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (
            !uri.IsAbsoluteUri
            || uri.Scheme != Uri.UriSchemeHttps
            || !uri.IsDefaultPort
            || uri.UserInfo.Length != 0
            || uri.Fragment.Length != 0
        )
            throw new ArgumentException("Unsupported problem URL.", nameof(uri));
        if (
            uri.IdnHost == "atcoder.jp"
            && Regex.IsMatch(
                uri.AbsolutePath,
                @"^/contests/(?:abc|arc|agc)[0-9]+/tasks/[a-z0-9_]+$",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)
            )
            && (uri.Query.Length == 0 || uri.Query is "?lang=ja" or "?lang=en")
        )
            return new Uri($"https://atcoder.jp{uri.AbsolutePath}");
        if (
            uri.IdnHost is "onlinemathcontest.com" or "www.onlinemathcontest.com"
            && uri.Query.Length == 0
            && Regex.IsMatch(
                uri.AbsolutePath,
                @"^/contests/(?:omcb|omce|omc)[0-9]+/tasks/[0-9]+$",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)
            )
        )
            return new Uri($"https://onlinemathcontest.com{uri.AbsolutePath}");
        throw new ArgumentException("Unsupported problem URL.", nameof(uri));
    }

    public async Task<ProblemMetadata> GetAsync(
        Uri uri,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = Normalize(uri);
        if (normalized.IdnHost == "atcoder.jp")
        {
            var result = await atCoder.GetAsync(normalized, cancellationToken);
            return new(
                result.ProblemUrl,
                result.Title,
                ProblemSource.AtCoder,
                result.Category,
                result.ProblemIndex,
                result.ContestId,
                result.ContestName,
                result.ContestStartDate
            );
        }
        var omcResult = await omc.GetAsync(normalized, cancellationToken);
        return new(
            omcResult.ProblemUrl,
            omcResult.Title,
            ProblemSource.OnlineMathContest,
            omcResult.Category,
            omcResult.ProblemIndex,
            omcResult.ContestId,
            omcResult.ContestName,
            omcResult.ContestStartDate
        );
    }
}
