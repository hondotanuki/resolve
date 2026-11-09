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
using Resolve.Infrastructure.External.OMC;

namespace Resolve.Api.Tests;

public sealed class OMCProblemRegistrationIntegrationTests
{
    [Theory]
    [InlineData('B', HttpStatusCode.Created, 2)]
    [InlineData('H', HttpStatusCode.UnprocessableEntity, 1)]
    public async Task 実際のProviderと模擬HTMLを使い登録または未対応として扱う(
        char index,
        HttpStatusCode expected,
        int expectedRequests
    )
    {
        using var root = new CustomWebApplicationFactory<Program>();
        var clock = new FakeClock();
        var handler = new FakeOMCHandler(clock, index);
        using var factory = root.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IExternalRequestClock>();
                services.AddSingleton<IExternalRequestClock>(clock);
                services.RemoveAll<ExternalHttpClient>();
                services.AddSingleton(_ => new ExternalHttpClient(clock, handler));
                services.RemoveAll<IProblemMetadataProvider>();
                services.AddSingleton<IProblemMetadataProvider, ProblemMetadataProvider>();
            })
        );
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://onlinemathcontest.com/contests/omcb087/tasks/5454" }
        );
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(expectedRequests, handler.Starts.Count);
        var items = await client.GetFromJsonAsync<JsonElement>("/learning-items");
        if (expected == HttpStatusCode.Created)
        {
            Assert.Single(items.EnumerateArray());
            Assert.Equal("OMCB087(B)", items[0].GetProperty("title").GetString());
            Assert.Equal("2026-10-20", items[0].GetProperty("contestStartDate").GetString());
            Assert.True(handler.Starts[1] - handler.Starts[0] >= TimeSpan.FromSeconds(2).Ticks);
        }
        else
        {
            Assert.Empty(items.EnumerateArray());
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

    private sealed class FakeOMCHandler(FakeClock clock, char index) : HttpMessageHandler
    {
        public List<long> Starts { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Starts.Add(clock.GetTimestamp());
            var html = request.RequestUri!.AbsolutePath.Contains(
                "/tasks/",
                StringComparison.Ordinal
            )
                ? $"<h1 id=\"problem-title\">OMCB087({index})</h1>"
                : "<h1>OMCB087</h1><div><p class=\"list-group-item-heading\">2026-10-20 (火) 21:00</p><p class=\"list-group-item-text\">Begin Time</p></div>";
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent(html, Encoding.UTF8, "text/html"),
                }
            );
        }
    }
}
