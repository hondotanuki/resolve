using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class UpdateLearningItemTitleApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UpdateLearningItemTitleApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task タイトルを変更して詳細から確認できる()
    {
        var created = await _client.PostAsJsonAsync(
            "/learning-items",
            new
            {
                title = "元のタイトル",
                source = "AtCoder",
                category = "Beginner",
                problemIndex = "A",
            }
        );
        var item = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = item.GetProperty("id").GetInt32();

        var response = await _client.PatchAsJsonAsync(
            $"/learning-items/{id}",
            new { title = "新しいタイトル" }
        );
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var detail = await _client.GetFromJsonAsync<JsonElement>($"/learning-items/{id}");
        Assert.Equal("新しいタイトル", detail.GetProperty("title").GetString());
    }

    [Fact]
    public async Task 空白タイトルは拒否し既存の値を保持する()
    {
        var created = await _client.PostAsJsonAsync(
            "/learning-items",
            new
            {
                title = "元のタイトル",
                source = "AtCoder",
                category = "Beginner",
                problemIndex = "A",
            }
        );
        var item = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = item.GetProperty("id").GetInt32();

        var response = await _client.PatchAsJsonAsync(
            $"/learning-items/{id}",
            new { title = "  " }
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var detail = await _client.GetFromJsonAsync<JsonElement>($"/learning-items/{id}");
        Assert.Equal("元のタイトル", detail.GetProperty("title").GetString());
    }

    [Fact]
    public async Task 存在しない問題は404を返す()
    {
        var response = await _client.PatchAsJsonAsync(
            "/learning-items/99999",
            new { title = "新しいタイトル" }
        );
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
