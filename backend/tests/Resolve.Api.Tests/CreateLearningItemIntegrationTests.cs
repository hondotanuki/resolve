using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Api.Tests;

public sealed class CreateLearningItemIntegrationTests
{
    [Fact]
    public async Task URLから登録し同じ問題は外部取得せず既存のIDを返す()
    {
        using var root = new CustomWebApplicationFactory<Program>();
        var provider = new FakeProvider();
        using var factory = root.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProblemMetadataProvider>();
                services.AddSingleton<IProblemMetadataProvider>(provider);
            })
        );
        using var client = factory.CreateClient();

        var first = await client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a" }
        );
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var firstItem = await first.Content.ReadFromJsonAsync<JsonElement>();
        var id = firstItem.GetProperty("id").GetInt32();
        var duplicate = await client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a?lang=en" }
        );
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        var duplicateItem = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(id, duplicateItem.GetProperty("id").GetInt32());
        Assert.Equal(1, provider.CallCount);
        var detail = await client.GetFromJsonAsync<JsonElement>($"/learning-items/{id}");
        Assert.Equal("abc482", detail.GetProperty("contestId").GetString());
        Assert.Equal("2026-10-31", detail.GetProperty("contestStartDate").GetString());
    }

    [Fact]
    public async Task 外部取得に失敗した場合は登録されない()
    {
        using var root = new CustomWebApplicationFactory<Program>();
        var provider = new FakeProvider { Fail = true };
        using var factory = root.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProblemMetadataProvider>();
                services.AddSingleton<IProblemMetadataProvider>(provider);
            })
        );
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a" }
        );
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("取得", error.GetProperty("title").GetString());
        var items = await client.GetFromJsonAsync<JsonElement>("/learning-items");
        Assert.Empty(items.EnumerateArray());
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task 未対応の問題区分は原因を返し登録しない()
    {
        using var root = new CustomWebApplicationFactory<Program>();
        var provider = new FakeProvider { UnsupportedCategory = true };
        using var factory = root.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProblemMetadataProvider>();
                services.AddSingleton<IProblemMetadataProvider>(provider);
            })
        );
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a" }
        );
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("目標時間", error.GetProperty("title").GetString());
        var items = await client.GetFromJsonAsync<JsonElement>("/learning-items");
        Assert.Empty(items.EnumerateArray());
    }

    [Fact]
    public async Task 外部取得のタイムアウトは504を返し登録しない()
    {
        using var root = new CustomWebApplicationFactory<Program>();
        var provider = new FakeProvider { Timeout = true };
        using var factory = root.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IProblemMetadataProvider>();
                services.AddSingleton<IProblemMetadataProvider>(provider);
            })
        );
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a" }
        );
        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
        var items = await client.GetFromJsonAsync<JsonElement>("/learning-items");
        Assert.Empty(items.EnumerateArray());
    }

    private sealed class FakeProvider : IProblemMetadataProvider
    {
        public int CallCount { get; private set; }
        public bool Fail { get; init; }
        public bool UnsupportedCategory { get; init; }
        public bool Timeout { get; init; }

        public Uri Normalize(Uri uri) => new("https://atcoder.jp/contests/abc482/tasks/abc482_a");

        public Task<ProblemMetadata> GetAsync(
            Uri uri,
            CancellationToken cancellationToken = default
        )
        {
            CallCount++;
            if (Fail)
                throw new HttpRequestException("External service unavailable.");
            if (UnsupportedCategory)
                throw new UnsupportedProblemCategoryException("unsupported category");
            if (Timeout)
                throw new TaskCanceledException("External request timed out.");
            return Task.FromResult(
                new ProblemMetadata(
                    uri,
                    "Nigiri",
                    ProblemSource.AtCoder,
                    ContestCategory.Beginner,
                    'A',
                    "abc482",
                    "ABC 482",
                    new DateOnly(2026, 10, 31)
                )
            );
        }
    }
}
