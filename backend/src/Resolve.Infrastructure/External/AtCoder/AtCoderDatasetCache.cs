using System.Text.Json;
using System.Text.Json.Serialization;
using Resolve.Infrastructure.External.Http;

namespace Resolve.Infrastructure.External.AtCoder;

public sealed class AtCoderDatasetCache : IDisposable
{
    private const string ResourceUrl = "https://kenkoooo.com/atcoder/resources/";
    private static readonly TimeSpan JapanOffset = TimeSpan.FromHours(9);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ExternalHttpClient _client;
    private readonly IExternalRequestClock _clock;
    private readonly string _cachePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AtCoderDatasetCache(
        ExternalHttpClient client,
        IExternalRequestClock clock,
        string cachePath
    )
    {
        _client = client;
        _clock = clock;
        _cachePath = cachePath;
    }

    internal async Task<CacheData> GetAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(_cachePath))
            {
                await using var existing = File.OpenRead(_cachePath);
                var cached = await JsonSerializer.DeserializeAsync<CacheData>(
                    existing,
                    JsonOptions,
                    cancellationToken
                );
                if (
                    cached is not null
                    && IsValid(cached)
                    && cached.FetchedAt <= _clock.UtcNow
                    && cached.FetchedAt >= LastThursday(_clock.UtcNow)
                )
                    return cached;
            }
            var fetchedAt = _clock.UtcNow;
            var problems = await DownloadAsync<Problem[]>(
                "merged-problems.json",
                cancellationToken
            );
            var mappings = await DownloadAsync<ContestProblem[]>(
                "contest-problem.json",
                cancellationToken
            );
            var contests = await DownloadAsync<Contest[]>("contests.json", cancellationToken);
            var updated = new CacheData(fetchedAt, problems, mappings, contests);
            if (!IsValid(updated))
                throw new InvalidDataException(
                    "AtCoder Problemsから取得したデータが不完全なため、既存のキャッシュを更新しませんでした。"
                );
            var directory =
                Path.GetDirectoryName(_cachePath)
                ?? throw new InvalidOperationException(
                    "AtCoder Problemsのキャッシュパスにはディレクトリを指定してください。"
                );
            Directory.CreateDirectory(directory);
            var temporaryPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var stream = File.Create(temporaryPath))
                    await JsonSerializer.SerializeAsync(
                        stream,
                        updated,
                        JsonOptions,
                        cancellationToken
                    );
                File.Move(temporaryPath, _cachePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            return updated;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<T> DownloadAsync<T>(string name, CancellationToken cancellationToken)
    {
        using var response = await _client.GetAsync(new Uri(ResourceUrl + name), cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException(
                $"AtCoder Problemsから取得したデータセット '{name}' はnullでした。"
            );
    }

    private static bool IsValid(CacheData cache) =>
        cache.FetchedAt != default
        && cache.Problems is { Length: > 0 }
        && cache.ContestProblems is { Length: > 0 }
        && cache.Contests is { Length: > 0 }
        && cache.Problems.All(x => x is not null && x.Id is not null && x.Name is not null)
        && cache.ContestProblems.All(x =>
            x is not null
            && x.ContestId is not null
            && x.ProblemId is not null
            && x.ProblemIndex is not null
        )
        && cache.Contests.All(x =>
            x is not null
            && x.Id is not null
            && x.Title is not null
            && x.StartEpochSecond is not null
        );

    private static DateTimeOffset LastThursday(DateTimeOffset now)
    {
        var local = now.ToOffset(JapanOffset);
        var daysSinceThursday = ((int)local.DayOfWeek - (int)DayOfWeek.Thursday + 7) % 7;
        return new DateTimeOffset(local.Year, local.Month, local.Day, 0, 0, 0, JapanOffset).AddDays(
            -daysSinceThursday
        );
    }

    public void Dispose() => _gate.Dispose();

    internal sealed record CacheData(
        DateTimeOffset FetchedAt,
        Problem[] Problems,
        ContestProblem[] ContestProblems,
        Contest[] Contests
    );

    internal sealed record Problem(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("name")] string? Name
    );

    internal sealed record ContestProblem(
        [property: JsonPropertyName("contest_id")] string? ContestId,
        [property: JsonPropertyName("problem_id")] string? ProblemId,
        [property: JsonPropertyName("problem_index")] string? ProblemIndex
    );

    internal sealed record Contest(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("start_epoch_second")] long? StartEpochSecond
    );
}
