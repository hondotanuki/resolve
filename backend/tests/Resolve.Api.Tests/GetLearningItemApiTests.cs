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
        var createRequest = new
        {
            itemType = "algorithm_problem",
            title = "二分探索",
            content = "二分探索を解く",
        };

        var createResponse = await _client.PostAsJsonAsync("/learning-items", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();
        var response = await _client.GetAsync($"/learning-items/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var createdAt = json.GetProperty("createdAt").GetDateTime();
        var updatedAt = json.GetProperty("updatedAt").GetDateTime();

        Assert.Equal(id, json.GetProperty("id").GetInt32());
        Assert.Equal("algorithm_problem", json.GetProperty("itemType").GetString());
        Assert.Equal("二分探索", json.GetProperty("title").GetString());
        Assert.Equal("二分探索を解く", json.GetProperty("content").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("archivedAt").ValueKind);
        Assert.NotEqual(default, createdAt);
        Assert.NotEqual(default, updatedAt);
    }

    [Fact]
    public async Task 存在しないIDなら404を返す()
    {
        var response = await _client.GetAsync("/learning-items/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ContentなしのLearningItemも取得できる()
    {
        var createRequest = new { itemType = "algorithm_problem", title = "二分探索" };
        var createResponse = await _client.PostAsJsonAsync("/learning-items", createRequest);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetInt32();
        var response = await _client.GetAsync($"/learning-items/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Null, json.GetProperty("content").ValueKind);
    }
}
