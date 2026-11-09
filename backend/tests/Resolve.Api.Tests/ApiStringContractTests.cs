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
    [InlineData("success")]
    [InlineData("failure")]
    public async Task 学習結果は既存の文字列で登録と更新ができる(string result)
    {
        var itemResponse = await TestProblemApi.CreateAsync(_client, "title");
        var item = await itemResponse.Content.ReadFromJsonAsync<JsonElement>();
        var itemId = item.GetProperty("id").GetInt32();
        var created = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result,
                studiedAt = "2026-11-01T00:00:00Z",
                durationSeconds = 0,
            }
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
            $"{{\"result\":{value},\"studiedAt\":\"2026-11-01T00:00:00Z\",\"durationSeconds\":0}}",
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
                "CreateStudyRecordRequest",
                "CreateStudyRecordResponse",
                "ListStudyRecordsResponse",
                "UpdateStudyRecordRequest",
                "UpdateStudyRecordResponse",
            }
        )
            AssertValues(schemas, name, "result", ["success", "failure"]);
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
