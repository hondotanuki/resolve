using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Api.Tests;

public sealed class CreateLearningItemFromUrlApiTests
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
            "/learning-items/from-url",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a" }
        );
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var firstItem = await first.Content.ReadFromJsonAsync<JsonElement>();
        var id = firstItem.GetProperty("id").GetInt32();
        var duplicate = await client.PostAsJsonAsync(
            "/learning-items/from-url",
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

    private sealed class FakeProvider : IProblemMetadataProvider
    {
        public int CallCount { get; private set; }

        public Uri Normalize(Uri uri) => new("https://atcoder.jp/contests/abc482/tasks/abc482_a");

        public Task<ProblemMetadata> GetAsync(
            Uri uri,
            CancellationToken cancellationToken = default
        )
        {
            CallCount++;
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
