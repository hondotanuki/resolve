using System.Net;
using System.Text;
using Resolve.Application.Interfaces;
using Resolve.Domain;
using Resolve.Infrastructure.External.Http;
using Resolve.Infrastructure.External.OMC;

namespace Resolve.Infrastructure.Tests;

public sealed class OMCProblemInfoProviderTests : IDisposable
{
    private readonly FakeClock _clock = new();
    private readonly FakeHandler _handler = new();
    private readonly ExternalHttpClient _client;
    private readonly OMCProblemInfoProvider _provider;

    public OMCProblemInfoProviderTests()
    {
        _client = new ExternalHttpClient(_clock, _handler);
        _handler.Clock = _clock;
        _provider = new OMCProblemInfoProvider(_client);
    }

    [Theory]
    [InlineData("omcb087", "5454", "B", "2026-10-20", ContestCategory.Beginner)]
    [InlineData("omc294", "18178", "A", "2026-10-16", ContestCategory.Regular)]
    [InlineData("omce028", "19075", "A", "2026-10-05", ContestCategory.Expert)]
    public async Task 問題ページとコンテストページから情報を取得する(
        string contestId,
        string problemId,
        string index,
        string date,
        ContestCategory category
    )
    {
        _handler.UseSampleResponses = true;
        var result = await _provider.GetAsync(
            new Uri($"https://onlinemathcontest.com/contests/{contestId}/tasks/{problemId}")
        );

        Assert.Equal(contestId, result.ContestId);
        Assert.Equal($"{contestId.ToUpperInvariant()}({index})", result.Title);
        Assert.Equal(contestId.ToUpperInvariant(), result.ContestName);
        Assert.Equal(
            DateOnly.ParseExact(
                date,
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture
            ),
            result.ContestStartDate
        );
        Assert.Equal(category, result.Category);
        Assert.Equal(index[0], result.ProblemIndex);
        Assert.Equal(
            $"https://onlinemathcontest.com/contests/{contestId}/tasks/{problemId}",
            result.ProblemUrl.ToString()
        );
        Assert.Equal(2, _handler.Paths.Count);
        Assert.True(_handler.Starts[1] - _handler.Starts[0] >= TimeSpan.FromSeconds(2).Ticks);
    }

    [Fact]
    public async Task 開始日の表示がなければ拒否する()
    {
        _handler.Date = "invalid";
        await Assert.ThrowsAsync<InvalidDataException>(() => _provider.GetAsync(ProblemUrl()));
    }

    [Fact]
    public async Task 異なるコンテストの問題タイトルは拒否する()
    {
        _handler.Title = "OMC294(A)";
        await Assert.ThrowsAsync<InvalidDataException>(() => _provider.GetAsync(ProblemUrl()));
        Assert.Single(_handler.Paths);
    }

    [Fact]
    public async Task 未対応の問題区分は拒否する()
    {
        _handler.Title = "OMCB087(H)";
        await Assert.ThrowsAsync<UnsupportedProblemCategoryException>(() =>
            _provider.GetAsync(ProblemUrl())
        );
        Assert.Single(_handler.Paths);
    }

    [Fact]
    public async Task コンテスト名が一致しないページは拒否する()
    {
        _handler.ContestName = "OMC294";
        await Assert.ThrowsAsync<InvalidDataException>(() => _provider.GetAsync(ProblemUrl()));
    }

    [Fact]
    public async Task 取得失敗時に再試行しない()
    {
        _handler.Status = HttpStatusCode.TooManyRequests;
        await Assert.ThrowsAsync<HttpRequestException>(() => _provider.GetAsync(ProblemUrl()));
        Assert.Single(_handler.Paths);
    }

    [Theory]
    [InlineData("https://example.com/contests/omcb087/tasks/5454")]
    [InlineData("https://onlinemathcontest.com/contests/omcb087/tasks/5454?foo=bar")]
    [InlineData("https://onlinemathcontest.com/contests/omct005/tasks/5454")]
    public async Task 未対応のURLでは通信しない(string url)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _provider.GetAsync(new Uri(url)));
        Assert.Empty(_handler.Paths);
    }

    private static Uri ProblemUrl() =>
        new("https://onlinemathcontest.com/contests/omcb087/tasks/5454");

    public void Dispose() => _client.Dispose();

    private sealed class FakeClock : IExternalRequestClock
    {
        private long _ticks;
        public DateTimeOffset UtcNow => DateTimeOffset.UnixEpoch.AddTicks(_ticks);

        public long GetTimestamp() => _ticks;

        public TimeSpan GetElapsedTime(long start, long finish) =>
            TimeSpan.FromTicks(finish - start);

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ticks += delay.Ticks;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        public FakeClock Clock { get; set; } = null!;
        public List<string> Paths { get; } = [];
        public List<long> Starts { get; } = [];
        public string Title { get; set; } = "OMCB087(B)";
        public string ContestName { get; set; } = "OMCB087";
        public string Date { get; set; } = "2026-10-20";
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public bool UseSampleResponses { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var path = request.RequestUri!.AbsolutePath;
            Paths.Add(path);
            Starts.Add(Clock.GetTimestamp());
            if (UseSampleResponses)
            {
                var fileName = path switch
                {
                    "/contests/omcb087/tasks/5454" => "omcb087-task.html",
                    "/contests/omcb087" => "omcb087-contest.html",
                    "/contests/omc294/tasks/18178" => "omc294-task.html",
                    "/contests/omc294" => "omc294-contest.html",
                    "/contests/omce028/tasks/19075" => "omce028-task.html",
                    "/contests/omce028" => "omce028-contest.html",
                    _ => throw new InvalidOperationException("想定外のOMCのURLです。"),
                };
                var fixturePath = Path.Combine(
                    AppContext.BaseDirectory,
                    "Fixtures",
                    "OMC",
                    fileName
                );
                var sampleHtml = File.ReadAllText(fixturePath);
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        RequestMessage = request,
                        Content = new StringContent(sampleHtml, Encoding.UTF8, "text/html"),
                    }
                );
            }
            var html = path.Contains("/tasks/", StringComparison.Ordinal)
                ? $"<html><body><h1 id=\"problem-title\">{Title}</h1></body></html>"
                : $"<html><body><h1>{ContestName}</h1><div class=\"bmd-list-group-col\"><p class=\"list-group-item-heading\">{Date} (火) 21:00</p><p class=\"list-group-item-text\">Begin Time</p></div></body></html>";
            return Task.FromResult(
                new HttpResponseMessage(Status)
                {
                    RequestMessage = request,
                    Content = new StringContent(html, Encoding.UTF8, "text/html"),
                }
            );
        }
    }
}
