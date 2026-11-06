using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class UpdateStudyRecordApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UpdateStudyRecordApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> CreateLearningItemAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/learning-items",
            new { itemType = "algorithm_problem", title = "二分探索" }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private async Task<int> CreateStudyRecordAsync(
        int learningItemId,
        string studiedAt,
        string result
    )
    {
        var response = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            new
            {
                studiedAt,
                result,
                durationSeconds = 120,
                memo = "元のメモ",
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private Task<HttpResponseMessage> PatchAsync(int id, string json)
    {
        return _client.PatchAsync(
            $"/study-records/{id}",
            new StringContent(json, Encoding.UTF8, "application/json")
        );
    }

    private async Task<JsonElement> GetRecordsAsync(int learningItemId)
    {
        var response = await _client.GetAsync($"/learning-items/{learningItemId}/study-records");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<string?> GetStatusAsync(int learningItemId)
    {
        var response = await _client.GetAsync($"/learning-items/{learningItemId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("status").GetString();
    }

    [Fact]
    public async Task 一部の項目だけ訂正し未指定の値を維持する()
    {
        var itemId = await CreateLearningItemAsync();
        var recordId = await CreateStudyRecordAsync(itemId, "2026-10-15T09:00:00Z", "failure");

        var response = await PatchAsync(recordId, """{"memo":"訂正後"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(recordId, updated.GetProperty("id").GetInt32());
        Assert.Equal(itemId, updated.GetProperty("learningItemId").GetInt32());
        Assert.Equal("failure", updated.GetProperty("result").GetString());
        Assert.Equal("2026-10-15T09:00:00Z", updated.GetProperty("studiedAt").GetString());
        Assert.Equal(120, updated.GetProperty("durationSeconds").GetInt32());
        Assert.Equal("訂正後", updated.GetProperty("memo").GetString());

        var records = await GetRecordsAsync(itemId);
        Assert.Equal(1, records.GetArrayLength());
        Assert.Equal("訂正後", records[0].GetProperty("memo").GetString());
        Assert.Equal("failure", records[0].GetProperty("result").GetString());
        Assert.Equal(120, records[0].GetProperty("durationSeconds").GetInt32());
    }

    [Fact]
    public async Task 任意項目は明示的なnullで消去できる()
    {
        var itemId = await CreateLearningItemAsync();
        var recordId = await CreateStudyRecordAsync(itemId, "2026-10-15T09:00:00Z", "success");

        var response = await PatchAsync(recordId, """{"durationSeconds":null,"memo":null}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Null, updated.GetProperty("durationSeconds").ValueKind);
        Assert.Equal(JsonValueKind.Null, updated.GetProperty("memo").ValueKind);
        var records = await GetRecordsAsync(itemId);
        Assert.Equal(JsonValueKind.Null, records[0].GetProperty("durationSeconds").ValueKind);
        Assert.Equal(JsonValueKind.Null, records[0].GetProperty("memo").ValueKind);
        Assert.Equal("success", records[0].GetProperty("result").GetString());
    }

    [Fact]
    public async Task UTCの日時をそのまま保存する()
    {
        var itemId = await CreateLearningItemAsync();
        var recordId = await CreateStudyRecordAsync(itemId, "2026-10-15T09:00:00Z", "success");
        const string expected = "2026-10-15T10:30:00Z";

        var response = await _client.PatchAsJsonAsync(
            $"/study-records/{recordId}",
            new { studiedAt = expected }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(expected, updated.GetProperty("studiedAt").GetString());
        var records = await GetRecordsAsync(itemId);
        Assert.Equal(expected, records[0].GetProperty("studiedAt").GetString());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"result":null}""")]
    [InlineData("""{"result":"invalid"}""")]
    [InlineData("""{"studiedAt":null}""")]
    [InlineData("""{"studiedAt":"2026-10-15T09:00:00"}""")]
    [InlineData("""{"studiedAt":"2026-10-15T18:00:00+09:00"}""")]
    [InlineData("""{"studiedAt":"2026-10-15T04:00:00-05:00"}""")]
    [InlineData("""{"studiedAt":"2026-10-15T09:00:00+00:00"}""")]
    [InlineData("""{"studiedAt":"invalid"}""")]
    [InlineData("""{"durationSeconds":-1}""")]
    [InlineData("""{"durationSeconds":"120"}""")]
    public async Task 不正な入力は400を返し記録を変更しない(string json)
    {
        var itemId = await CreateLearningItemAsync();
        var recordId = await CreateStudyRecordAsync(itemId, "2026-10-15T09:00:00Z", "failure");

        var response = await PatchAsync(recordId, json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var records = await GetRecordsAsync(itemId);
        Assert.Equal(1, records.GetArrayLength());
        Assert.Equal(recordId, records[0].GetProperty("id").GetInt32());
        Assert.Equal("failure", records[0].GetProperty("result").GetString());
        Assert.Equal("2026-10-15T09:00:00Z", records[0].GetProperty("studiedAt").GetString());
        Assert.Equal(120, records[0].GetProperty("durationSeconds").GetInt32());
        Assert.Equal("元のメモ", records[0].GetProperty("memo").GetString());
    }

    [Fact]
    public async Task 存在しない記録は404を返す()
    {
        var response = await PatchAsync(999999, """{"memo":"訂正後"}""");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OpenAPIに更新項目の省略とnullの契約が反映される()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();
        var patch = document
            .GetProperty("paths")
            .GetProperty("/study-records/{studyRecordId}")
            .GetProperty("patch");
        var responses = patch.GetProperty("responses");
        Assert.True(responses.TryGetProperty("200", out _));
        Assert.True(responses.TryGetProperty("400", out _));
        Assert.True(responses.TryGetProperty("404", out _));

        var requestSchema = document
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("UpdateStudyRecordRequest");
        var properties = requestSchema.GetProperty("properties");
        Assert.False(
            requestSchema.TryGetProperty("required", out var required)
                && required.GetArrayLength() != 0
        );
        Assert.Equal("string", properties.GetProperty("result").GetProperty("type").GetString());
        Assert.Equal("string", properties.GetProperty("studiedAt").GetProperty("type").GetString());
        Assert.Equal(
            "date-time",
            properties.GetProperty("studiedAt").GetProperty("format").GetString()
        );
        var durationTypes = properties
            .GetProperty("durationSeconds")
            .GetProperty("type")
            .EnumerateArray()
            .Select(x => x.GetString());
        Assert.Contains("integer", durationTypes);
        Assert.Contains("null", durationTypes);
        Assert.DoesNotContain("string", durationTypes);
        var memoTypes = properties
            .GetProperty("memo")
            .GetProperty("type")
            .EnumerateArray()
            .Select(x => x.GetString());
        Assert.Contains("string", memoTypes);
        Assert.Contains("null", memoTypes);
        Assert.False(properties.TryGetProperty("memoIsSpecified", out _));

        var createSchema = document
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("CreateStudyRecordRequest");
        var createStudiedAt = createSchema.GetProperty("properties").GetProperty("studiedAt");
        Assert.Equal("string", createStudiedAt.GetProperty("type").GetString());
        Assert.Equal("date-time", createStudiedAt.GetProperty("format").GetString());
        Assert.Contains(
            createSchema.GetProperty("required").EnumerateArray().Select(x => x.GetString()),
            name => name == "studiedAt"
        );
    }

    [Fact]
    public async Task 最新記録の結果を変更すると現在状態が変わる()
    {
        var itemId = await CreateLearningItemAsync();
        await CreateStudyRecordAsync(itemId, "2026-10-14T09:00:00Z", "success");
        var latestId = await CreateStudyRecordAsync(itemId, "2026-10-15T09:00:00Z", "failure");
        Assert.Equal("needs_review", await GetStatusAsync(itemId));

        var response = await PatchAsync(latestId, """{"result":"success"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("completed", await GetStatusAsync(itemId));
    }

    [Fact]
    public async Task 学習日時の変更で最新記録が入れ替わると現在状態が変わる()
    {
        var itemId = await CreateLearningItemAsync();
        var olderId = await CreateStudyRecordAsync(itemId, "2026-10-14T09:00:00Z", "success");
        await CreateStudyRecordAsync(itemId, "2026-10-15T09:00:00Z", "failure");
        Assert.Equal("needs_review", await GetStatusAsync(itemId));

        var response = await PatchAsync(olderId, """{"studiedAt":"2026-10-16T09:00:00Z"}""");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("completed", await GetStatusAsync(itemId));
        var records = await GetRecordsAsync(itemId);
        Assert.Equal(olderId, records[0].GetProperty("id").GetInt32());
    }
}
