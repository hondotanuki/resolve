using System.Text.RegularExpressions;
using Resolve.Domain;

namespace Resolve.Infrastructure.External;

public sealed partial class AtCoderProblemInfoProvider
{
    private static readonly TimeSpan JapanOffset = TimeSpan.FromHours(9);
    private readonly AtCoderDatasetCache _cache;

    public AtCoderProblemInfoProvider(AtCoderDatasetCache cache) => _cache = cache;

    public async Task<AtCoderProblemInfo> GetAsync(
        Uri uri,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (
            !uri.IsAbsoluteUri
            || uri.Scheme != Uri.UriSchemeHttps
            || uri.IdnHost != "atcoder.jp"
            || !uri.IsDefaultPort
            || uri.UserInfo.Length != 0
            || uri.Fragment.Length != 0
        )
            throw new ArgumentException(
                "The problem URL must use HTTPS on atcoder.jp without credentials, a custom port, or a fragment.",
                nameof(uri)
            );
        var match = ProblemPath().Match(uri.AbsolutePath);
        if (
            !match.Success
            || (uri.Query.Length != 0 && uri.Query is not "?lang=ja" and not "?lang=en")
        )
            throw new ArgumentException(
                "The problem URL must identify an ABC, ARC, or AGC task and may only include ?lang=ja or ?lang=en.",
                nameof(uri)
            );
        var contestId = match.Groups[1].Value;
        var problemId = match.Groups[2].Value;
        var cache = await _cache.GetAsync(cancellationToken);
        var problem = cache.Problems.SingleOrDefault(x => x.Id == problemId);
        var mapping = cache.ContestProblems.SingleOrDefault(x =>
            x.ContestId == contestId && x.ProblemId == problemId
        );
        var contest = cache.Contests.SingleOrDefault(x => x.Id == contestId);
        if (problem is null || string.IsNullOrWhiteSpace(problem.Name))
            throw new InvalidOperationException(
                $"AtCoder Problems does not contain a title for problem '{problemId}'."
            );
        if (mapping is null || mapping.ProblemIndex is not { Length: 1 })
            throw new InvalidOperationException(
                $"AtCoder Problems does not contain a valid problem index for '{problemId}' in contest '{contestId}'."
            );
        if (contest is null || string.IsNullOrWhiteSpace(contest.Title))
            throw new InvalidOperationException(
                $"AtCoder Problems does not contain a title for contest '{contestId}'."
            );
        var category = contestId[..3] switch
        {
            "abc" => ContestCategory.Beginner,
            "arc"
                when contest.Title.Contains(
                    "Regular Contest++",
                    StringComparison.OrdinalIgnoreCase
                ) => ContestCategory.RegularPlus,
            "arc" => ContestCategory.Regular,
            "agc" => ContestCategory.Grand,
            _ => throw new InvalidOperationException($"Unsupported AtCoder contest '{contestId}'."),
        };
        var index = mapping.ProblemIndex[0];
        if (TargetTime.GetSeconds(ProblemSource.AtCoder, category, index) is null)
            throw new InvalidOperationException(
                $"No target time is defined for contest '{contestId}' and problem index '{index}'."
            );
        if (contest.StartEpochSecond is null)
            throw new InvalidOperationException(
                $"AtCoder Problems does not contain a start date for contest '{contestId}'."
            );
        DateOnly date;
        try
        {
            date = DateOnly.FromDateTime(
                DateTimeOffset
                    .FromUnixTimeSeconds(contest.StartEpochSecond.Value)
                    .ToOffset(JapanOffset)
                    .DateTime
            );
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new InvalidOperationException(
                $"AtCoder Problems returned an invalid start date for contest '{contestId}'.",
                exception
            );
        }
        return new AtCoderProblemInfo(
            new Uri($"https://atcoder.jp/contests/{contestId}/tasks/{problemId}"),
            problem.Name,
            contestId,
            contest.Title,
            date,
            category,
            index
        );
    }

    [GeneratedRegex(
        @"^/contests/((?:abc|arc|agc)[0-9]+)/tasks/([a-z0-9_]+)$",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex ProblemPath();
}
