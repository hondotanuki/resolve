using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Resolve.Api.Tests;

public sealed class CreateStudyRecordApiTests
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    public CreateStudyRecordApiTests(CustomWebApplicationFactory<Program> factory)
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
            content = "二分探索を解く",
        };

        var response = await _client.PostAsJsonAsync("/learning-items", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    [Theory]
    [InlineData("""{"Result":"success","studiedAt":"2026-10-15T09:00:00Z"}""")]
    [InlineData("""{"result":"success","StudiedAt":"2026-10-15T09:00:00Z"}""")]
    public async Task 必須項目の名前が大文字小文字まで一致しなければ400を返す(string json)
    {
        var learningItemId = await CreateLearningItemAsync();
        using var request = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(
            $"/learning-items/{learningItemId}/study-records",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 正常なJSONなら登録結果を返す()
    {
        var learningItemId = await CreateLearningItemAsync();
        var createStudyRecordRequest = new
        {
            Result = "success",
            StudiedAt,
            DurationSeconds = 0,
        };

        var createdStudyRecordResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            createStudyRecordRequest
        );
        var createdStudyRecordJson =
            await createdStudyRecordResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.Created, createdStudyRecordResponse.StatusCode);
        var studyRecordId = createdStudyRecordJson.GetProperty("id").GetInt32();
        Assert.True(studyRecordId > 0);
        Assert.Equal(
            $"/study-records/{studyRecordId}",
            createdStudyRecordResponse.Headers.Location?.ToString()
        );
        Assert.Equal(
            learningItemId,
            createdStudyRecordJson.GetProperty("learningItemId").GetInt32()
        );
        Assert.Equal("success", createdStudyRecordJson.GetProperty("result").GetString());
        Assert.Equal("completed", createdStudyRecordJson.GetProperty("status").GetString());
        Assert.Equal(StudiedAt, createdStudyRecordJson.GetProperty("studiedAt").GetDateTime());
        Assert.Equal(0, createdStudyRecordJson.GetProperty("durationSeconds").GetInt32());
        Assert.Equal(JsonValueKind.Null, createdStudyRecordJson.GetProperty("memo").ValueKind);
    }

    [Fact]
    public async Task 任意項目を含む正常なJSONでも登録結果を返す()
    {
        var learningItemId = await CreateLearningItemAsync();
        var createStudyRecordRequest = new
        {
            Result = "success",
            StudiedAt,
            DurationSeconds = 2,
            Memo = "メモ",
        };

        var createdStudyRecordResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            createStudyRecordRequest
        );
        var createdStudyRecordJson =
            await createdStudyRecordResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Created, createdStudyRecordResponse.StatusCode);
        Assert.Equal(
            learningItemId,
            createdStudyRecordJson.GetProperty("learningItemId").GetInt32()
        );
        Assert.Equal("success", createdStudyRecordJson.GetProperty("result").GetString());
        Assert.Equal("completed", createdStudyRecordJson.GetProperty("status").GetString());
        Assert.Equal(2, createdStudyRecordJson.GetProperty("durationSeconds").GetInt32());
        Assert.Equal("メモ", createdStudyRecordJson.GetProperty("memo").GetString());
        Assert.Equal(StudiedAt, createdStudyRecordJson.GetProperty("studiedAt").GetDateTime());
    }

    [Fact]
    public async Task FailureならNeedsReviewを返す()
    {
        var learningItemId = await CreateLearningItemAsync();
        var request = new
        {
            Result = "failure",
            StudiedAt,
            DurationSeconds = 0,
        };

        var response = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            request
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("failure", json.GetProperty("result").GetString());
        Assert.Equal("needs_review", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task 存在しないIDなら404を返す()
    {
        var createStudyRecordRequest = new
        {
            Result = "success",
            StudiedAt,
            DurationSeconds = 0,
        };
        var createdStudyRecordResponse = await _client.PostAsJsonAsync(
            "/learning-items/999999/study-records",
            createStudyRecordRequest
        );
        Assert.Equal(HttpStatusCode.NotFound, createdStudyRecordResponse.StatusCode);
    }

    [Fact]
    public async Task 不正なResultなら400を返す()
    {
        var learningItemId = await CreateLearningItemAsync();
        var createStudyRecordRequest = new { Result = "invalid", StudiedAt };
        var createdStudyRecordResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            createStudyRecordRequest
        );
        Assert.Equal(HttpStatusCode.BadRequest, createdStudyRecordResponse.StatusCode);
    }

    [Fact]
    public async Task 負の所要時間なら400を返す()
    {
        var learningItemId = await CreateLearningItemAsync();
        var createStudyRecordRequest = new
        {
            Result = "success",
            StudiedAt,
            DurationSeconds = -1,
        };
        var createdStudyRecordResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            createStudyRecordRequest
        );
        Assert.Equal(HttpStatusCode.BadRequest, createdStudyRecordResponse.StatusCode);
    }

    [Fact]
    public async Task 不正な日付なら400を返す()
    {
        var createStudyRecordRequest = new { Result = "success", StudiedAt = "invalid" };
        var createdStudyRecordResponse = await _client.PostAsJsonAsync(
            "/learning-items/1/study-records",
            createStudyRecordRequest
        );
        Assert.Equal(HttpStatusCode.BadRequest, createdStudyRecordResponse.StatusCode);
    }

    [Fact]
    public async Task タイムゾーンのない学習日時なら400を返す()
    {
        var learningItemId = await CreateLearningItemAsync();
        var request = new { result = "success", studiedAt = "2026-10-15T09:00:00" };

        var response = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("2026-10-15T18:00:00+09:00")]
    [InlineData("2026-10-15T04:00:00-05:00")]
    [InlineData("2026-10-15T09:00:00+00:00")]
    public async Task UTC以外の日時は400を返す(string studiedAt)
    {
        var learningItemId = await CreateLearningItemAsync();
        var request = new { result = "success", studiedAt };

        var response = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 所要時間に数値文字列は使用できない()
    {
        var learningItemId = await CreateLearningItemAsync();
        var request = new
        {
            result = "success",
            studiedAt = StudiedAt,
            durationSeconds = "120",
        };

        var response = await _client.PostAsJsonAsync(
            $"/learning-items/{learningItemId}/study-records",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
