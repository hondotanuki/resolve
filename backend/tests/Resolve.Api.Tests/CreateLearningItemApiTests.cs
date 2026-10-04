using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class CreateLearningItemApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CreateLearningItemApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task 正常なJSONなら登録結果を返す()
    {
        var request = new
        {
            itemType = "algorithm_problem",
            title = "二分探索",
            content = "二分探索の問題を解く",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.GetProperty("id").GetInt32();
        Assert.True(id > 0);
        Assert.Equal("algorithm_problem", json.GetProperty("itemType").GetString());
        Assert.Equal("二分探索", json.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("archivedAt").ValueKind);
        Assert.Equal($"/learning-items/{id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Contentなしでも201を返す()
    {
        var request = new { itemType = "algorithm_problem", title = "二分探索" };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task 空タイトルなら400を返す()
    {
        var request = new
        {
            itemType = "algorithm_problem",
            title = "",
            content = "二分探索の問題を解く",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 空白だけのタイトルなら400を返す()
    {
        var request = new
        {
            itemType = "algorithm_problem",
            title = "   ",
            content = "二分探索の問題を解く",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 未定義のItemTypeなら400を返す()
    {
        var request = new
        {
            itemType = "invalid_type",
            title = "二分探索",
            content = "二分探索の問題を解く",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
