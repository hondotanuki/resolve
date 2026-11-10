using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Resolve.Application.Interfaces;
using Resolve.Infrastructure.External;
using Resolve.Infrastructure.External.AtCoder;
using Resolve.Infrastructure.External.Http;

namespace Resolve.Api.Tests;

public sealed class AtCoderSharedProblemRegistrationTests
{
    [Fact]
    public async Task 同じ問題でもコンテストごとに登録し目標時間を分ける()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "resolve-shared-problem-tests",
            Guid.NewGuid().ToString("N")
        );
        try
        {
            using var root = new CustomWebApplicationFactory<Program>();
            var clock = new FakeClock();
            var handler = new DatasetHandler();
            using var factory = root.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IExternalRequestClock>();
                    services.AddSingleton<IExternalRequestClock>(clock);
                    services.RemoveAll<ExternalHttpClient>();
                    services.AddSingleton(_ => new ExternalHttpClient(clock, handler));
                    services.RemoveAll<AtCoderDatasetCache>();
                    services.AddSingleton(sp => new AtCoderDatasetCache(
                        sp.GetRequiredService<ExternalHttpClient>(),
                        clock,
                        Path.Combine(directory, "datasets.json")
                    ));
                    services.RemoveAll<IProblemMetadataProvider>();
                    services.AddSingleton<IProblemMetadataProvider, ProblemMetadataProvider>();
                })
            );
            using var client = factory.CreateClient();

            var first = await client.PostAsJsonAsync(
                "/learning-items",
                new { url = "https://atcoder.jp/contests/abc043/tasks/arc059_a" }
            );
            var second = await client.PostAsJsonAsync(
                "/learning-items",
                new { url = "https://atcoder.jp/contests/arc059/tasks/arc059_a" }
            );
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);
            Assert.Equal(HttpStatusCode.Created, second.StatusCode);
            var firstId = (await first.Content.ReadFromJsonAsync<JsonElement>())
                .GetProperty("id")
                .GetInt32();
            var secondId = (await second.Content.ReadFromJsonAsync<JsonElement>())
                .GetProperty("id")
                .GetInt32();
            Assert.NotEqual(firstId, secondId);

            var firstDetail = await client.GetFromJsonAsync<JsonElement>(
                $"/learning-items/{firstId}"
            );
            var secondDetail = await client.GetFromJsonAsync<JsonElement>(
                $"/learning-items/{secondId}"
            );
            Assert.Equal("abc043", firstDetail.GetProperty("contestId").GetString());
            Assert.Equal("arc059", secondDetail.GetProperty("contestId").GetString());
            Assert.Equal("C", firstDetail.GetProperty("problemIndex").GetString());
            Assert.Equal("C", secondDetail.GetProperty("problemIndex").GetString());
            Assert.Equal(20 * 60, firstDetail.GetProperty("targetSeconds").GetInt32());
            Assert.Equal(50 * 60, secondDetail.GetProperty("targetSeconds").GetInt32());
            Assert.Equal(
                2,
                (await client.GetFromJsonAsync<JsonElement>("/learning-items")).GetArrayLength()
            );
            Assert.Equal(3, handler.Count);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

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

    // ローカルに保存したAtCoder Problemsのデータから対象レコードを抽出。
    private sealed class DatasetHandler : HttpMessageHandler
    {
        public int Count { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Count++;
            var json = request.RequestUri!.AbsolutePath.Split('/')[^1] switch
            {
                "merged-problems.json" => """[{"id":"arc059_a","name":"Be Together"}]""",
                "contest-problem.json" =>
                    """[{"contest_id":"abc043","problem_id":"arc059_a","problem_index":"C"},{"contest_id":"arc059","problem_id":"arc059_a","problem_index":"C"}]""",
                "contests.json" =>
                    """[{"id":"abc043","title":"AtCoder Beginner Contest 043","start_epoch_second":1471089600},{"id":"arc059","title":"AtCoder Regular Contest 059","start_epoch_second":1471089600}]""",
                _ => throw new InvalidOperationException("想定外のデータセットです。"),
            };
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                }
            );
        }
    }
}
