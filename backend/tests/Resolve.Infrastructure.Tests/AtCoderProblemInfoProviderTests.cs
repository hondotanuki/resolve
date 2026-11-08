using System.Net;
using System.Text;
using Resolve.Domain;
using Resolve.Infrastructure.External;

namespace Resolve.Infrastructure.Tests;

public sealed class AtCoderProblemInfoProviderTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "resolve-atcoder-tests",
        Guid.NewGuid().ToString("N")
    );
    private readonly FakeClock _clock = new(
        new DateTimeOffset(2026, 11, 4, 14, 59, 0, TimeSpan.Zero)
    );
    private readonly FakeHandler _handler = new();
    private readonly ExternalHttpClient _client;
    private readonly AtCoderDatasetCache _cache;
    private readonly AtCoderProblemInfoProvider _provider;

    public AtCoderProblemInfoProviderTests()
    {
        _client = new ExternalHttpClient(_clock, _handler);
        _cache = new AtCoderDatasetCache(
            _client,
            _clock,
            Path.Combine(_directory, "datasets.json")
        );
        _provider = new AtCoderProblemInfoProvider(_cache);
    }

    [Theory]
    [InlineData("abc482", "abc482_a", "Nigiri", ContestCategory.Beginner, 'A')]
    [InlineData("arc233", "arc233_a", "Strange Subset Sum", ContestCategory.RegularPlus, 'A')]
    [InlineData("agc078", "agc078_a", "Rearrange ABC", ContestCategory.Grand, 'A')]
    [InlineData("abc043", "arc059_a", "Common", ContestCategory.Beginner, 'C')]
    [InlineData("arc059", "arc059_a", "Common", ContestCategory.Regular, 'C')]
    public async Task コンテストと問題の組から情報を取得する(
        string contest,
        string problem,
        string title,
        ContestCategory category,
        char index
    )
    {
        var result = await _provider.GetAsync(
            new Uri($"https://atcoder.jp/contests/{contest}/tasks/{problem}?lang=en")
        );
        Assert.Equal(title, result.Title);
        Assert.Equal(contest, result.ContestId);
        Assert.Equal(category, result.Category);
        Assert.Equal(index, result.ProblemIndex);
        Assert.Equal(new DateOnly(2026, 10, 31), result.ContestStartDate);
        Assert.Equal(
            $"https://atcoder.jp/contests/{contest}/tasks/{problem}",
            result.ProblemUrl.ToString()
        );
        Assert.Equal(3, _handler.Count);
    }

    [Fact]
    public async Task 木曜日の境界まではキャッシュを使い越えたら更新する()
    {
        var uri = new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a");
        await _provider.GetAsync(uri);
        await _provider.GetAsync(uri);
        Assert.Equal(3, _handler.Count);
        _clock.Advance(TimeSpan.FromMinutes(2));
        await _provider.GetAsync(uri);
        Assert.Equal(6, _handler.Count);
        await _provider.GetAsync(uri);
        Assert.Equal(6, _handler.Count);
    }

    [Fact]
    public async Task 更新失敗時は既存キャッシュを保持する()
    {
        var uri = new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a");
        await _provider.GetAsync(uri);
        var path = Path.Combine(_directory, "datasets.json");
        var previous = await File.ReadAllBytesAsync(path);
        _clock.Advance(TimeSpan.FromDays(8));
        _handler.Fail = true;
        await Assert.ThrowsAsync<HttpRequestException>(() => _provider.GetAsync(uri));
        Assert.Equal(previous, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task 不完全な取得結果では既存キャッシュを上書きしない()
    {
        var uri = new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a");
        await _provider.GetAsync(uri);
        var path = Path.Combine(_directory, "datasets.json");
        var previous = await File.ReadAllBytesAsync(path);
        _clock.Advance(TimeSpan.FromDays(8));
        _handler.Incomplete = true;
        await Assert.ThrowsAsync<InvalidDataException>(() => _provider.GetAsync(uri));
        Assert.Equal(previous, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task 一部のレコードに必須項目がない場合もキャッシュを上書きしない()
    {
        var uri = new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a");
        await _provider.GetAsync(uri);
        var path = Path.Combine(_directory, "datasets.json");
        var previous = await File.ReadAllBytesAsync(path);
        _clock.Advance(TimeSpan.FromDays(8));
        _handler.PartiallyIncomplete = true;
        await Assert.ThrowsAsync<InvalidDataException>(() => _provider.GetAsync(uri));
        Assert.Equal(previous, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task 再生成したProviderもローカルキャッシュを使える()
    {
        var uri = new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a");
        await _provider.GetAsync(uri);
        using var anotherCache = new AtCoderDatasetCache(
            _client,
            _clock,
            Path.Combine(_directory, "datasets.json")
        );
        var another = new AtCoderProblemInfoProvider(anotherCache);
        var result = await another.GetAsync(uri);
        Assert.Equal("Nigiri", result.Title);
        Assert.Equal(3, _handler.Count);
    }

    [Fact]
    public async Task 一致しない問題は推測しない()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _provider.GetAsync(new Uri("https://atcoder.jp/contests/abc482/tasks/unknown"))
        );
    }

    [Theory]
    [InlineData("https://example.com/contests/abc482/tasks/abc482_a")]
    [InlineData("https://atcoder.jp/contests/abc482/tasks/abc482_a?other=value")]
    public async Task 不正なURLでは通信しない(string url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _provider.GetAsync(new Uri(url)));
        Assert.Equal(0, _handler.Count);
    }

    public void Dispose()
    {
        _cache.Dispose();
        _client.Dispose();
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    private sealed class FakeClock(DateTimeOffset now) : IExternalRequestClock
    {
        private long _ticks;
        public DateTimeOffset UtcNow => now.AddTicks(_ticks);

        public long GetTimestamp() => _ticks;

        public TimeSpan GetElapsedTime(long start, long finish) =>
            TimeSpan.FromTicks(finish - start);

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Advance(delay);
            return Task.CompletedTask;
        }

        public void Advance(TimeSpan time) => _ticks += time.Ticks;
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public int Count { get; private set; }
        public bool Fail { get; set; }
        public bool Incomplete { get; set; }
        public bool PartiallyIncomplete { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Count++;
            if (Fail)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            if (Incomplete)
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("[{}]", Encoding.UTF8, "application/json"),
                    }
                );
            if (
                PartiallyIncomplete
                && request.RequestUri!.AbsolutePath.EndsWith(
                    "merged-problems.json",
                    StringComparison.Ordinal
                )
            )
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            "[{\"id\":\"abc482_a\",\"name\":\"Nigiri\"},{\"id\":\"broken\"}]",
                            Encoding.UTF8,
                            "application/json"
                        ),
                    }
                );
            var epoch = new DateTimeOffset(
                2026,
                10,
                31,
                12,
                0,
                0,
                TimeSpan.Zero
            ).ToUnixTimeSeconds();
            var json = request.RequestUri!.AbsolutePath.Split('/')[^1] switch
            {
                "merged-problems.json" =>
                    """[{"id":"abc482_a","name":"Nigiri"},{"id":"arc233_a","name":"Strange Subset Sum"},{"id":"agc078_a","name":"Rearrange ABC"},{"id":"arc059_a","name":"Common"}]""",
                "contest-problem.json" =>
                    """[{"contest_id":"abc482","problem_id":"abc482_a","problem_index":"A"},{"contest_id":"arc233","problem_id":"arc233_a","problem_index":"A"},{"contest_id":"agc078","problem_id":"agc078_a","problem_index":"A"},{"contest_id":"abc043","problem_id":"arc059_a","problem_index":"C"},{"contest_id":"arc059","problem_id":"arc059_a","problem_index":"C"}]""",
                "contests.json" =>
                    $$"""[{"id":"abc482","title":"AtCoder Beginner Contest 482","start_epoch_second":{{epoch}}},{"id":"arc233","title":"AtCoder Regular Contest++ 233","start_epoch_second":{{epoch}}},{"id":"agc078","title":"AtCoder Grand Contest 078","start_epoch_second":{{epoch}}},{"id":"abc043","title":"AtCoder Beginner Contest 043","start_epoch_second":{{epoch}}},{"id":"arc059","title":"AtCoder Regular Contest 059","start_epoch_second":{{epoch}}}]""",
                _ => throw new InvalidOperationException("Unexpected dataset request."),
            };
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                }
            );
        }
    }
}
