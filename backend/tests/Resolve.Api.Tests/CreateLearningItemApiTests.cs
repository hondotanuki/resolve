using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class CreateLearningItemApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CreateLearningItemApiTests(CustomWebApplicationFactory<Program> factory)
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
    public async Task 正常なJSONなら登録結果を返す()
    {
        var request = new
        {
            source = "AtCoder",
            category = "Beginner",
            problemIndex = "A",
            title = "二分探索",
            content = "二分探索の問題を解く",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.GetProperty("id").GetInt32();
        Assert.True(id > 0);

        Assert.Equal("二分探索", json.GetProperty("title").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("archivedAt").ValueKind);
        Assert.Equal($"/learning-items/{id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task 必須項目のみで201を返す()
    {
        var request = new
        {
            source = "AtCoder",
            category = "Beginner",
            problemIndex = "A",
            title = "二分探索",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task 必須項目の名前が大文字小文字まで一致しなければ400を返す()
    {
        using var request = new StringContent(
            """{"Title":"二分探索","source":"AtCoder","category":"Beginner","problemIndex":"A"}""",
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 任意項目の名前が一致しなければ未知の項目として無視する()
    {
        using var request = new StringContent(
            """{"title":"二分探索","Content":"本文","source":"AtCoder","category":"Beginner","problemIndex":"A"}""",
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/learning-items", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var detail = await _client.GetFromJsonAsync<JsonElement>(
            $"/learning-items/{created.GetProperty("id").GetInt32()}"
        );

        Assert.False(detail.TryGetProperty("content", out _));
    }

    [Fact]
    public async Task 空タイトルなら400を返す()
    {
        var request = new
        {
            source = "AtCoder",
            category = "Beginner",
            problemIndex = "A",
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
            source = "AtCoder",
            category = "Beginner",
            problemIndex = "A",
            title = "   ",
            content = "二分探索の問題を解く",
        };
        var response = await _client.PostAsJsonAsync("/learning-items", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
