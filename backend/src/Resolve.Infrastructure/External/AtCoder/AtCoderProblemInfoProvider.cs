using System.Text.RegularExpressions;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Infrastructure.External.AtCoder;

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
                "問題URLには認証情報・独自ポート・フラグメントを含まないatcoder.jpのHTTPS URLを指定してください。",
                nameof(uri)
            );
        var match = ProblemPath().Match(uri.AbsolutePath);
        if (
            !match.Success
            || (uri.Query.Length != 0 && uri.Query is not "?lang=ja" and not "?lang=en")
        )
            throw new ArgumentException(
                "ABC、ARC、AGCの問題URLを指定してください。クエリには?lang=jaまたは?lang=enのみ指定できます。",
                nameof(uri)
            );
        var contestId = match.Groups[1].Value;
        var problemId = match.Groups[2].Value;
        var cache = await _cache.GetAsync(cancellationToken);
        AtCoderDatasetCache.Problem? problem;
        AtCoderDatasetCache.ContestProblem? mapping;
        AtCoderDatasetCache.Contest? contest;
        try
        {
            problem = cache.Problems.SingleOrDefault(x => x.Id == problemId);
            mapping = cache.ContestProblems.SingleOrDefault(x =>
                x.ContestId == contestId && x.ProblemId == problemId
            );
            contest = cache.Contests.SingleOrDefault(x => x.Id == contestId);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidDataException(
                "AtCoder Problemsから取得した問題情報に重複があります。",
                exception
            );
        }
        if (problem is null || string.IsNullOrWhiteSpace(problem.Name))
            throw new InvalidDataException(
                $"AtCoder Problemsから問題 '{problemId}' のタイトルを取得できませんでした。"
            );
        if (mapping is null || mapping.ProblemIndex is not { Length: 1 })
            throw new InvalidDataException(
                $"AtCoder Problemsからコンテスト '{contestId}' の問題 '{problemId}' の有効な問題記号を取得できませんでした。"
            );
        if (contest is null || string.IsNullOrWhiteSpace(contest.Title))
            throw new InvalidDataException(
                $"AtCoder Problemsからコンテスト '{contestId}' の名称を取得できませんでした。"
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
            _ => throw new InvalidDataException(
                $"対応していないAtCoderのコンテストです: '{contestId}'。"
            ),
        };
        var index = mapping.ProblemIndex[0];
        if (TargetTime.GetSeconds(ProblemSource.AtCoder, category, index) is null)
            throw new UnsupportedProblemCategoryException(
                $"コンテスト '{contestId}' の問題 '{index}' に対応する目標時間がありません。"
            );
        if (contest.StartEpochSecond is null)
            throw new InvalidDataException(
                $"AtCoder Problemsからコンテスト '{contestId}' の開始日を取得できませんでした。"
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
            throw new InvalidDataException(
                $"AtCoder Problemsから取得したコンテスト '{contestId}' の開始日が不正です。",
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
