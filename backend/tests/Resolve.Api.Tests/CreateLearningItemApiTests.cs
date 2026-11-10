using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class CreateLearningItemApiTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task 有効なURLなら登録し詳細を参照できる()
    {
        var response = await TestProblemApi.CreateAsync(_client, "二分探索");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = result.GetProperty("id").GetInt32();
        Assert.Equal($"/learning-items/{id}", response.Headers.Location?.ToString());
        var detail = await _client.GetFromJsonAsync<JsonElement>($"/learning-items/{id}");
        Assert.Equal("二分探索", detail.GetProperty("title").GetString());
        Assert.Equal("abc482", detail.GetProperty("contestId").GetString());
        Assert.Equal("2026-10-31", detail.GetProperty("contestStartDate").GetString());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"url\":null}")]
    [InlineData("{\"Url\":\"https://atcoder.jp/contests/abc482/tasks/abc482_a\"}")]
    [InlineData(
        "{\"title\":\"問題\",\"source\":\"AtCoder\",\"category\":\"Beginner\",\"problemIndex\":\"A\"}"
    )]
    [InlineData("{\"url\":\"not a url\"}")]
    public async Task 無効な入力や旧入力は拒否する(string json)
    {
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/learning-items", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var items = await _client.GetFromJsonAsync<JsonElement>("/learning-items");
        Assert.Empty(items.EnumerateArray());
    }

    [Fact]
    public async Task 暫定URL登録エンドポイントは廃止されている()
    {
        var response = await _client.PostAsJsonAsync(
            "/learning-items/from-url",
            new { url = "https://atcoder.jp/contests/abc482/tasks/abc482_a" }
        );
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task 対象外URLには原因を返す()
    {
        var response = await _client.PostAsJsonAsync(
            "/learning-items",
            new { url = "https://example.com/problem" }
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("URL", error.GetProperty("title").GetString());
    }
}
