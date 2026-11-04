using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class ListStudyRecordsApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    public ListStudyRecordsApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> CreateLearningItemAsync()
    {
        var request = new { itemType = "algorithm_problem", title = "二分探索" };
        var response = await _client.PostAsJsonAsync("/learning-items", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private async Task<int> CreateStudyRecordAsync(
        int learningItemId,
        DateTime studiedAt,
        string result = "success",
        int? durationSeconds = null,
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

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task 登録済みLearningItemのStudyRecordが一覧で返る()
    {
        var learningItemId = await CreateLearningItemAsync();
        var studyRecordId = await CreateStudyRecordAsync(
            learningItemId,
            StudiedAt,
            result: "failure"
        );

        // StudyRecordの一覧取得
        var listStudyRecordsResponse = await _client.GetAsync(
            $"/learning-items/{learningItemId}/study-records"
        );
        Assert.Equal(HttpStatusCode.OK, listStudyRecordsResponse.StatusCode);
        var listStudyRecordsjson =
            await listStudyRecordsResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, listStudyRecordsjson.ValueKind);
        Assert.Equal(1, listStudyRecordsjson.GetArrayLength());

        // 登録したStudyRecordが返ることを確認
        var listStudyRecord = listStudyRecordsjson[0];
        Assert.Equal(studyRecordId, listStudyRecord.GetProperty("id").GetInt32());
        Assert.Equal(learningItemId, listStudyRecord.GetProperty("learningItemId").GetInt32());
        Assert.Equal("failure", listStudyRecord.GetProperty("result").GetString());
        Assert.Equal(StudiedAt, listStudyRecord.GetProperty("studiedAt").GetDateTime());
        Assert.Equal("2026-10-15T09:00:00Z", listStudyRecord.GetProperty("studiedAt").GetString());
    }

    [Fact]
    public async Task LearningItemに履歴がないなら空の配列が返る()
    {
        var learningItemId = await CreateLearningItemAsync();

        // StudyRecordの一覧取得
        var listStudyRecordsResponse = await _client.GetAsync(
            $"/learning-items/{learningItemId}/study-records"
        );
        Assert.Equal(HttpStatusCode.OK, listStudyRecordsResponse.StatusCode);
        var listStudyRecordsjson =
            await listStudyRecordsResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, listStudyRecordsjson.ValueKind);
        Assert.Equal(0, listStudyRecordsjson.GetArrayLength());
    }

    [Fact]
    public async Task LearningItem自体がないなら404が返る()
    {
        // StudyRecordの一覧取得
        var listStudyRecordsResponse = await _client.GetAsync($"/learning-items/999/study-records");
        Assert.Equal(HttpStatusCode.NotFound, listStudyRecordsResponse.StatusCode);
    }

    [Fact]
    public async Task 学習日時が新しい記録から順に返る()
    {
        var learningItemId = await CreateLearningItemAsync();
        // 新しい日時の記録を先に登録し、ID順と日時順を逆にする。
        var newerId = await CreateStudyRecordAsync(learningItemId, StudiedAt.AddDays(1));
        var olderId = await CreateStudyRecordAsync(learningItemId, StudiedAt);
        Assert.True(olderId > newerId);

        var response = await _client.GetAsync($"/learning-items/{learningItemId}/study-records");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(2, json.GetArrayLength());
        Assert.Equal(newerId, json[0].GetProperty("id").GetInt32());
        Assert.Equal(olderId, json[1].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task 学習日時が同じならIDの大きい記録から順に返る()
    {
        var learningItemId = await CreateLearningItemAsync();
        var firstId = await CreateStudyRecordAsync(learningItemId, StudiedAt);
        var secondId = await CreateStudyRecordAsync(learningItemId, StudiedAt);
        Assert.True(secondId > firstId);

        var response = await _client.GetAsync($"/learning-items/{learningItemId}/study-records");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(2, json.GetArrayLength());
        Assert.Equal(secondId, json[0].GetProperty("id").GetInt32());
        Assert.Equal(firstId, json[1].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task 別のLearningItemの記録は一覧に含まれない()
    {
        var learningItemId = await CreateLearningItemAsync();
        var otherLearningItemId = await CreateLearningItemAsync();
        Assert.NotEqual(learningItemId, otherLearningItemId);

        var recordId = await CreateStudyRecordAsync(
            learningItemId,
            StudiedAt,
            result: "failure",
            durationSeconds: 120,
            memo: "もう一度復習"
        );
        await CreateStudyRecordAsync(otherLearningItemId, StudiedAt.AddDays(1));

        var response = await _client.GetAsync($"/learning-items/{learningItemId}/study-records");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(1, json.GetArrayLength());
        Assert.Equal(recordId, json[0].GetProperty("id").GetInt32());
        Assert.Equal(learningItemId, json[0].GetProperty("learningItemId").GetInt32());
        Assert.Equal("failure", json[0].GetProperty("result").GetString());
        Assert.Equal(120, json[0].GetProperty("durationSeconds").GetInt32());
        Assert.Equal("もう一度復習", json[0].GetProperty("memo").GetString());
    }
}
