using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class InputConstraintsApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public InputConstraintsApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task OpenAPIに入力の制約と説明が反映される()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var schemas = document.GetProperty("components").GetProperty("schemas");
        var title = schemas
            .GetProperty("CreateLearningItemRequest")
            .GetProperty("properties")
            .GetProperty("title");
        Assert.Equal(1, title.GetProperty("minLength").GetInt32());
        Assert.Contains("空白", title.GetProperty("description").GetString());

        foreach (var name in new[] { "CreateStudyRecordRequest", "UpdateStudyRecordRequest" })
        {
            var properties = schemas.GetProperty(name).GetProperty("properties");
            var duration = properties.GetProperty("durationSeconds");
            Assert.Equal(0, duration.GetProperty("minimum").GetInt32());
            Assert.Equal(int.MaxValue, duration.GetProperty("maximum").GetInt32());
            Assert.Equal("integer", duration.GetProperty("type").GetString());
            var date = properties.GetProperty("studiedAt");
            Assert.Equal("date-time", date.GetProperty("format").GetString());
            Assert.Contains("UTC", date.GetProperty("description").GetString());
            Assert.Contains("初期値", date.GetProperty("description").GetString());
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public async Task 所要時間の境界値は登録と更新で受け付ける(int seconds)
    {
        var itemId = await CreateItemAsync();
        var created = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result = "success",
                studiedAt = "2026-11-01T00:00:00Z",
                durationSeconds = seconds,
            }
        );
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var record = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(seconds, record.GetProperty("durationSeconds").GetInt32());
        var patched = await _client.PatchAsJsonAsync(
            $"/study-records/{record.GetProperty("id").GetInt32()}",
            new { durationSeconds = seconds }
        );
        Assert.Equal(HttpStatusCode.OK, patched.StatusCode);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("2147483648")]
    [InlineData("1.5")]
    public async Task 範囲外または小数の所要時間は登録と更新で拒否する(string seconds)
    {
        var itemId = await CreateItemAsync();
        var created = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result = "success",
                studiedAt = "2026-11-01T00:00:00Z",
                durationSeconds = 0,
            }
        );
        var record = await created.Content.ReadFromJsonAsync<JsonElement>();
        var createBody = Json(
            $"{{\"result\":\"success\",\"studiedAt\":\"2026-11-01T00:00:00Z\",\"durationSeconds\":{seconds}}}"
        );
        var updateBody = Json($"{{\"durationSeconds\":{seconds}}}");
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await _client.PostAsync($"/learning-items/{itemId}/study-records", createBody)
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await _client.PatchAsync(
                    $"/study-records/{record.GetProperty("id").GetInt32()}",
                    updateBody
                )
            ).StatusCode
        );
    }

    [Fact]
    public async Task 初期値の日時は登録と更新で拒否する()
    {
        var itemId = await CreateItemAsync();
        var created = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result = "success",
                studiedAt = "2026-11-01T00:00:00Z",
                durationSeconds = 0,
            }
        );
        var record = await created.Content.ReadFromJsonAsync<JsonElement>();
        const string initialDate = "0001-01-01T00:00:00Z";
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await _client.PostAsJsonAsync(
                    $"/learning-items/{itemId}/study-records",
                    new { result = "success", studiedAt = initialDate }
                )
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await _client.PatchAsJsonAsync(
                    $"/study-records/{record.GetProperty("id").GetInt32()}",
                    new { studiedAt = initialDate }
                )
            ).StatusCode
        );
    }

    private async Task<int> CreateItemAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/learning-items",
            new
            {
                itemType = "english_word",
                source = "AtCoder",
                category = "Beginner",
                problemIndex = "A",
                title = "a",
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id")
            .GetInt32();
    }

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");
}
