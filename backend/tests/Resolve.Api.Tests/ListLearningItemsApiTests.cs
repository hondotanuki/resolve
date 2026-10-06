using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class ListLearningItemApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ListLearningItemApiTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task 登録済みLearningItemが一覧で返る()
    {
        var createRequest = new
        {
            itemType = "algorithm_problem",
            title = "二分探索",
            content = "二分探索を解く",
        };

        await _client.PostAsJsonAsync("/learning-items", createRequest);
        var response = await _client.GetAsync("/learning-items");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);

        var item = json[0];
        Assert.True(item.GetProperty("id").GetInt32() > 0);
        Assert.Equal("algorithm_problem", item.GetProperty("itemType").GetString());
        Assert.Equal("二分探索", item.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("archivedAt").ValueKind);
    }

    [Fact]
    public async Task データがなければ空配列を返す()
    {
        var response = await _client.GetAsync("/learning-items");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(0, json.GetArrayLength());
    }
}
