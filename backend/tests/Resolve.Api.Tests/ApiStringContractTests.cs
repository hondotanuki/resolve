using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class ApiStringContractTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiStringContractTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData("algorithm_problem")]
    [InlineData("english_word")]
    [InlineData("english_article")]
    public async Task 学習対象の種別は既存の文字列で送受信できる(string itemType)
    {
        var response = await _client.PostAsJsonAsync(
            "/learning-items",
            new { itemType, title = "title" }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(itemType, json.GetProperty("itemType").GetString());
    }

    [Theory]
    [InlineData("\"ALGORITHM_PROBLEM\"")]
    [InlineData("\"unknown\"")]
    [InlineData("0")]
    [InlineData("null")]
    public async Task 不正な学習対象の種別は拒否する(string value)
    {
        var body = new StringContent(
            $"{{\"itemType\":{value},\"title\":\"title\"}}",
            Encoding.UTF8,
            "application/json"
        );
        var response = await _client.PostAsync("/learning-items", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("success")]
    [InlineData("failure")]
    [InlineData("reviewed")]
    public async Task 学習結果は既存の文字列で登録と更新ができる(string result)
    {
        var itemResponse = await _client.PostAsJsonAsync(
            "/learning-items",
            new { itemType = "english_word", title = "title" }
        );
        var item = await itemResponse.Content.ReadFromJsonAsync<JsonElement>();
        var itemId = item.GetProperty("id").GetInt32();
        var created = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new { result, studiedAt = "2026-11-01T00:00:00Z" }
        );
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var json = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(result, json.GetProperty("result").GetString());
        var updated = await _client.PatchAsJsonAsync(
            $"/study-records/{json.GetProperty("id").GetInt32()}",
            new { result }
        );
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedJson = await updated.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(result, updatedJson.GetProperty("result").GetString());
    }

    [Theory]
    [InlineData("\"SUCCESS\"")]
    [InlineData("\"unknown\"")]
    [InlineData("0")]
    [InlineData("null")]
    public async Task 不正な学習結果は登録と更新で拒否する(string value)
    {
        var create = new StringContent(
            $"{{\"result\":{value},\"studiedAt\":\"2026-11-01T00:00:00Z\"}}",
            Encoding.UTF8,
            "application/json"
        );
        var patch = new StringContent($"{{\"result\":{value}}}", Encoding.UTF8, "application/json");
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await _client.PostAsync("/learning-items/1/study-records", create)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await _client.PatchAsync("/study-records/1", patch)).StatusCode
        );
    }

    [Fact]
    public async Task OpenAPIに各RequestResponseの選択肢が反映される()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var schemas = document.GetProperty("components").GetProperty("schemas");
        foreach (
            var name in new[]
            {
                "CreateLearningItemRequest",
                "CreateLearningItemResponse",
                "ListLearningItemsResponse",
                "GetLearningItemResponse",
            }
        )
            AssertValues(
                schemas,
                name,
                "itemType",
                ["algorithm_problem", "english_word", "english_article"]
            );
        foreach (
            var name in new[]
            {
                "CreateStudyRecordRequest",
                "CreateStudyRecordResponse",
                "ListStudyRecordsResponse",
                "UpdateStudyRecordRequest",
                "UpdateStudyRecordResponse",
            }
        )
            AssertValues(schemas, name, "result", ["success", "failure", "reviewed"]);
        foreach (var name in new[] { "GetLearningItemResponse", "CreateStudyRecordResponse" })
            AssertValues(schemas, name, "status", ["unstarted", "needs_review", "completed"]);
    }

    private static void AssertValues(
        JsonElement schemas,
        string name,
        string property,
        string[] expected
    )
    {
        var schema = schemas.GetProperty(name).GetProperty("properties").GetProperty(property);
        Assert.Equal("string", schema.GetProperty("type").GetString());
        Assert.Equal(
            expected,
            schema.GetProperty("enum").EnumerateArray().Select(value => value.GetString()).ToArray()
        );
    }
}
