using System.Net;
using System.Net.Http.Headers;
using Resolve.Infrastructure.External.Http;

namespace Resolve.Infrastructure.Tests;

public sealed class ExternalHttpClientTests
{
    private static readonly Uri AtCoder = new("https://atcoder.jp/contests/abc482");
    private static readonly Uri OMC = new("https://onlinemathcontest.com/contests/omcb087");

    [Fact]
    public async Task 同じ提供元への同時要求は開始間隔が2秒以上になる()
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new ExternalHttpClient(
            clock,
            new BlockingHandler(clock, starts, release)
        );
        var first = client.GetAsync(AtCoder);
        var pending = Enumerable.Range(0, 4).Select(_ => client.GetAsync(AtCoder)).ToArray();
        Assert.All(pending, task => Assert.False(task.IsCompleted));
        release.SetResult();
        var responses = await Task.WhenAll(pending.Prepend(first));
        foreach (var response in responses)
            response.Dispose();

        Assert.Equal(5, starts.Count);
        Assert.All(
            starts.Zip(starts.Skip(1)),
            pair => Assert.True(pair.Second - pair.First >= TimeSpan.FromSeconds(2).Ticks)
        );
    }

    [Fact]
    public async Task 別の提供元は独立して制御される()
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        using var client = CreateClient(
            clock,
            _ =>
            {
                starts.Add(clock.GetTimestamp());
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        );

        using var first = await client.GetAsync(AtCoder);
        using var second = await client.GetAsync(OMC);
        Assert.Equal(starts[0], starts[1]);
    }

    [Fact]
    public async Task 一方の提供元の通信中も別の提供元には送信できる()
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new ExternalHttpClient(
            clock,
            new BlockingHandler(clock, starts, release)
        );
        var first = client.GetAsync(AtCoder);
        using var second = await client.GetAsync(OMC);
        Assert.False(first.IsCompleted);
        Assert.Equal(2, starts.Count);
        release.SetResult();
        using var completed = await first;
    }

    [Fact]
    public async Task リダイレクトにも間隔を適用する()
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        using var client = CreateClient(
            clock,
            _ =>
            {
                starts.Add(clock.GetTimestamp());
                if (starts.Count != 1)
                    return new HttpResponseMessage(HttpStatusCode.OK);
                var response = new HttpResponseMessage(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri("/contests/abc482/tasks", UriKind.Relative);
                return response;
            }
        );

        using var result = await client.GetAsync(AtCoder);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(starts[1] - starts[0] >= TimeSpan.FromSeconds(2).Ticks);
    }

    [Theory]
    [InlineData("https://example.com/private")]
    [InlineData("http://atcoder.jp/")]
    [InlineData("https://atcoder.jp:444/")]
    public async Task 許可されないリダイレクト先には送信しない(string location)
    {
        var count = 0;
        using var client = CreateClient(
            new FakeClock(),
            _ =>
            {
                count++;
                var response = new HttpResponseMessage(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri(location);
                return response;
            }
        );

        await Assert.ThrowsAsync<ArgumentException>(() => client.GetAsync(AtCoder));
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task リダイレクト回数が上限を超えたら中断する()
    {
        var count = 0;
        using var client = CreateClient(
            new FakeClock(),
            _ =>
            {
                count++;
                var response = new HttpResponseMessage(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri("/again", UriKind.Relative);
                return response;
            }
        );
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync(AtCoder));
        Assert.Equal(3, count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RetryAfterの秒数または日時まで次の送信を待つ(bool useDate)
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        using var client = CreateClient(
            clock,
            _ =>
            {
                starts.Add(clock.GetTimestamp());
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = useDate
                    ? new RetryConditionHeaderValue(clock.UtcNow.AddSeconds(10))
                    : new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
                return response;
            }
        );

        using var first = await client.GetAsync(AtCoder);
        using var second = await client.GetAsync(AtCoder);
        Assert.Equal(TimeSpan.FromSeconds(10).Ticks, starts[1] - starts[0]);
    }

    [Fact]
    public async Task RetryAfterが2秒未満でも通常の開始間隔を守る()
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        using var client = CreateClient(
            clock,
            _ =>
            {
                starts.Add(clock.GetTimestamp());
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = new RetryConditionHeaderValue(
                    TimeSpan.FromSeconds(1)
                );
                return response;
            }
        );
        using var first = await client.GetAsync(AtCoder);
        Assert.Single(starts);
        using var second = await client.GetAsync(AtCoder);
        Assert.Equal(TimeSpan.FromSeconds(2).Ticks, starts[1] - starts[0]);
    }

    [Fact]
    public async Task 送信失敗後も開始間隔を守る()
    {
        var clock = new FakeClock();
        var starts = new List<long>();
        using var client = CreateClient(
            clock,
            _ =>
            {
                starts.Add(clock.GetTimestamp());
                if (starts.Count == 1)
                    throw new HttpRequestException("failure");
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        );
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync(AtCoder));
        using var response = await client.GetAsync(AtCoder);
        Assert.Equal(TimeSpan.FromSeconds(2).Ticks, starts[1] - starts[0]);
    }

    [Fact]
    public async Task 待機中のキャンセル後も次の要求が実行できる()
    {
        var clock = new FakeClock { BlockDelay = true };
        var count = 0;
        using var client = CreateClient(
            clock,
            _ =>
            {
                count++;
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        );
        using var first = await client.GetAsync(AtCoder);
        using var cancellation = new CancellationTokenSource();
        var waiting = client.GetAsync(AtCoder, cancellation.Token);
        await clock.DelayStarted.Task;
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
        clock.BlockDelay = false;
        using var second = await client.GetAsync(AtCoder);
        Assert.Equal(2, count);
        Assert.True(clock.GetTimestamp() >= TimeSpan.FromSeconds(2).Ticks);
    }

    private static ExternalHttpClient CreateClient(
        FakeClock clock,
        Func<HttpRequestMessage, HttpResponseMessage> send
    ) => new(clock, new FakeHandler(send));

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(send(request));
    }

    private sealed class BlockingHandler(
        FakeClock clock,
        List<long> starts,
        TaskCompletionSource release
    ) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            starts.Add(clock.GetTimestamp());
            if (starts.Count == 1)
                await release.Task.WaitAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class FakeClock : IExternalRequestClock
    {
        private long _ticks;
        public bool BlockDelay { get; set; }
        public TaskCompletionSource DelayStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public DateTimeOffset UtcNow =>
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddTicks(GetTimestamp());

        public long GetTimestamp() => Interlocked.Read(ref _ticks);

        public TimeSpan GetElapsedTime(long start, long end) => TimeSpan.FromTicks(end - start);

        public async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            if (BlockDelay)
            {
                DelayStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Add(ref _ticks, delay.Ticks);
        }
    }
}
