using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class GetLearningItemApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GetLearningItemApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task 登録済みIDなら詳細を返す()
    {
        var createResponse = await TestProblemApi.CreateAsync(_client, "二分探索");
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();
        var response = await _client.GetAsync($"/learning-items/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var createdAt = json.GetProperty("createdAt").GetDateTime();
        var updatedAt = json.GetProperty("updatedAt").GetDateTime();

        Assert.Equal(id, json.GetProperty("id").GetInt32());

        Assert.Equal("二分探索", json.GetProperty("title").GetString());
        Assert.Equal(600, json.GetProperty("targetSeconds").GetInt32());
        Assert.False(json.TryGetProperty("content", out _));
        Assert.Equal("unstarted", json.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("archivedAt").ValueKind);
        Assert.NotEqual(default, createdAt);
        Assert.NotEqual(default, updatedAt);
        Assert.EndsWith("Z", json.GetProperty("createdAt").GetString());
        Assert.EndsWith("Z", json.GetProperty("updatedAt").GetString());
    }

    [Fact]
    public async Task 存在しないIDなら404を返す()
    {
        var response = await _client.GetAsync("/learning-items/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task 詳細に廃止したContentを含まない()
    {
        var createResponse = await TestProblemApi.CreateAsync(_client, "二分探索");

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();
        var response = await _client.GetAsync($"/learning-items/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(json.TryGetProperty("content", out _));
    }

    [Theory]
    [InlineData("failure", "needs_review")]
    [InlineData("success", "completed")]
    public async Task 最新の学習結果に応じた現在状態を返す(string result, string expectedStatus)
    {
        var createItemResponse = await TestProblemApi.CreateAsync(_client, "二分探索");
        Assert.Equal(HttpStatusCode.Created, createItemResponse.StatusCode);
        var item = await createItemResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = item.GetProperty("id").GetInt32();
        var newer = new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

        var newerResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{id}/study-records",
            new
            {
                result,
                studiedAt = newer,
                durationSeconds = 0,
            }
        );
        Assert.Equal(HttpStatusCode.Created, newerResponse.StatusCode);

        // 後から異なる結果の古い記録を登録し、登録順ではなく学習日時を検証する。
        var olderResult = result == "failure" ? "success" : "failure";
        var olderResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{id}/study-records",
            new
            {
                result = olderResult,
                studiedAt = newer.AddDays(-1),
                durationSeconds = 0,
            }
        );
        Assert.Equal(HttpStatusCode.Created, olderResponse.StatusCode);

        var response = await _client.GetAsync($"/learning-items/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expectedStatus, json.GetProperty("status").GetString());
    }
}
