using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class DeleteStudyRecordApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    public DeleteStudyRecordApiTests(CustomWebApplicationFactory<Program> factory)
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

    private async Task<int> CreateLearningItemAsync()
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
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private async Task<int> CreateStudyRecordAsync(
        int learningItemId,
        DateTime studiedAt,
        string result = "success",
        int durationSeconds = 0,
        string? memo = null
    )
    {
        var request = new
        {
            result,
            studiedAt,
            durationSeconds,
            memo,
        };
        var response = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            request
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private async Task<string?> GetLearningItemStatusAsync(int learningItemId)
    {
        var response = await _client.GetAsync($"/learning-items/{learningItemId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("status").GetString();
    }

    [Fact]
    public async Task 登録済みの学習記録を削除すると履歴から消える()
    {
        var learningItemId = await CreateLearningItemAsync();
        var studyRecordId = await CreateStudyRecordAsync(
            learningItemId,
            StudiedAt,
            result: "failure"
        );

        var beforeResponse = await _client.GetAsync(
            $"/learning-items/{learningItemId}/study-records"
        );
        Assert.Equal(HttpStatusCode.OK, beforeResponse.StatusCode);
        var beforeRecords = await beforeResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, beforeRecords.ValueKind);
        Assert.Equal(1, beforeRecords.GetArrayLength());
        Assert.Equal(studyRecordId, beforeRecords[0].GetProperty("id").GetInt32());

        var deleteResponse = await _client.DeleteAsync($"/study-records/{studyRecordId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterResponse = await _client.GetAsync(
            $"/learning-items/{learningItemId}/study-records"
        );
        Assert.Equal(HttpStatusCode.OK, afterResponse.StatusCode);
        var afterRecords = await afterResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, afterRecords.ValueKind);
        Assert.Equal(0, afterRecords.GetArrayLength());
    }

    [Fact]
    public async Task 存在しない学習記録を削除すると404が返る()
    {
        var response = await _client.DeleteAsync("/study-records/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task 最新の記録を削除すると残った最新記録から状態が算出される()
    {
        var learningItemId = await CreateLearningItemAsync();
        var olderId = await CreateStudyRecordAsync(learningItemId, StudiedAt, result: "success");
        var latestId = await CreateStudyRecordAsync(
            learningItemId,
            StudiedAt.AddDays(1),
            result: "failure"
        );

        Assert.Equal("needs_review", await GetLearningItemStatusAsync(learningItemId));

        var deleteResponse = await _client.DeleteAsync($"/study-records/{latestId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal("completed", await GetLearningItemStatusAsync(learningItemId));

        var listResponse = await _client.GetAsync(
            $"/learning-items/{learningItemId}/study-records"
        );
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var records = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, records.GetArrayLength());
        Assert.Equal(olderId, records[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task 最後の記録を削除すると未学習になる()
    {
        var learningItemId = await CreateLearningItemAsync();
        var recordId = await CreateStudyRecordAsync(learningItemId, StudiedAt, result: "success");

        Assert.Equal("completed", await GetLearningItemStatusAsync(learningItemId));

        var deleteResponse = await _client.DeleteAsync($"/study-records/{recordId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal("unstarted", await GetLearningItemStatusAsync(learningItemId));
    }
}
