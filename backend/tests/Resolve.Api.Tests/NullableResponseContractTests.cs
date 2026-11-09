using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class NullableResponseContractTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public NullableResponseContractTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task 全Responseにnull可能な項目がnullでも出力される()
    {
        var createdItem = await TestProblemApi.CreateAsync(_client, "title");
        Assert.Equal(HttpStatusCode.Created, createdItem.StatusCode);
        var item = await createdItem.Content.ReadFromJsonAsync<JsonElement>();
        var itemId = item.GetProperty("id").GetInt32();

        var items = await _client.GetFromJsonAsync<JsonElement>("/learning-items");
        Assert.Single(items.EnumerateArray());
        AssertNullProperties(items[0], "archivedAt");
        var detail = await _client.GetFromJsonAsync<JsonElement>($"/learning-items/{itemId}");
        AssertNullProperties(detail, "archivedAt");
        Assert.False(detail.TryGetProperty("content", out _));

        var createdRecord = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result = "success",
                studiedAt = "2026-11-01T00:00:00Z",
                durationSeconds = 0,
            }
        );
        Assert.Equal(HttpStatusCode.Created, createdRecord.StatusCode);
        var record = await createdRecord.Content.ReadFromJsonAsync<JsonElement>();
        AssertNullProperties(record, "memo");
        var recordId = record.GetProperty("id").GetInt32();

        var records = await _client.GetFromJsonAsync<JsonElement>(
            $"/learning-items/{itemId}/study-records"
        );
        Assert.Single(records.EnumerateArray());
        AssertNullProperties(records[0], "memo");
        var updated = await _client.PatchAsJsonAsync(
            $"/study-records/{recordId}",
            new { result = "failure" }
        );
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedRecord = await updated.Content.ReadFromJsonAsync<JsonElement>();
        AssertNullProperties(updatedRecord, "memo");
    }

    [Fact]
    public async Task OpenAPIではResponseのnull可能項目が必須でRequestは省略可能()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        var schemas = document.GetProperty("components").GetProperty("schemas");
        AssertRequiredNullable(schemas, "ListLearningItemsResponse", "archivedAt");
        AssertRequiredNullable(schemas, "GetLearningItemResponse", "archivedAt");
        AssertRequiredNullable(schemas, "CreateStudyRecordResponse", "memo");
        AssertRequiredNullable(schemas, "ListStudyRecordsResponse", "memo");
        AssertRequiredNullable(schemas, "UpdateStudyRecordResponse", "memo");
        Assert.False(
            schemas
                .GetProperty("CreateLearningItemRequest")
                .GetProperty("properties")
                .TryGetProperty("content", out _)
        );
        Assert.False(
            schemas
                .GetProperty("GetLearningItemResponse")
                .GetProperty("properties")
                .TryGetProperty("content", out _)
        );
        AssertOptionalNullable(schemas, "CreateStudyRecordRequest", "memo");
        AssertOptionalNullable(schemas, "UpdateStudyRecordRequest", "memo");
    }

    private static void AssertNullProperties(JsonElement json, params string[] names)
    {
        foreach (var name in names)
            Assert.Equal(JsonValueKind.Null, json.GetProperty(name).ValueKind);
    }

    private static void AssertRequiredNullable(
        JsonElement schemas,
        string name,
        params string[] fields
    )
    {
        var schema = schemas.GetProperty(name);
        var required = schema
            .GetProperty("required")
            .EnumerateArray()
            .Select(value => value.GetString())
            .ToArray();
        foreach (var field in fields)
        {
            Assert.Contains(field, required);
            AssertNullable(schema, field);
        }
    }

    private static void AssertOptionalNullable(
        JsonElement schemas,
        string name,
        params string[] fields
    )
    {
        var schema = schemas.GetProperty(name);
        var required = schema.TryGetProperty("required", out var requiredArray)
            ? requiredArray.EnumerateArray().Select(value => value.GetString()).ToArray()
            : [];
        foreach (var field in fields)
        {
            Assert.DoesNotContain(field, required);
            AssertNullable(schema, field);
        }
    }

    private static void AssertNullable(JsonElement schema, string field)
    {
        var types = schema.GetProperty("properties").GetProperty(field).GetProperty("type");
        Assert.Contains(types.EnumerateArray(), type => type.GetString() == "null");
    }
}
