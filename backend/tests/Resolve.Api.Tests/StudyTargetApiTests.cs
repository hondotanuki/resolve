using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Resolve.Domain;
using Resolve.Infrastructure.Persistence;

namespace Resolve.Api.Tests;

public sealed class StudyTargetApiTests(CustomWebApplicationFactory<Program> factory)
    : IClassFixture<CustomWebApplicationFactory<Program>>,
        IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task 記録時の目標時間が保存され訂正後の状態に反映される()
    {
        var itemResponse = await _client.PostAsJsonAsync(
            "/learning-items",
            new
            {
                itemType = "algorithm_problem",
                title = "問題",
                source = "AtCoder",
                category = "Beginner",
                problemIndex = "A",
            }
        );
        Assert.Equal(HttpStatusCode.Created, itemResponse.StatusCode);
        var item = await itemResponse.Content.ReadFromJsonAsync<JsonElement>();
        int itemId = item.GetProperty("id").GetInt32();
        Assert.Equal("AtCoder", item.GetProperty("source").GetString());
        Assert.Equal("Beginner", item.GetProperty("category").GetString());
        Assert.Equal("A", item.GetProperty("problemIndex").GetString());

        var createResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result = "success",
                studiedAt = "2026-11-01T00:00:00Z",
                durationSeconds = 600,
            }
        );
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(600, created.GetProperty("targetSeconds").GetInt32());
        Assert.Equal("completed", created.GetProperty("status").GetString());
        int id = created.GetProperty("id").GetInt32();

        // 問題区分の変更で現在の目標時間を10分から20分にする。
        // 問題編集APIは後続で実装するため、テスト用DBで直接変更する。
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ResolveDbContext>();
            var learningItem = await db.LearningItems.SingleAsync(x => x.Id == itemId);
            learningItem.SetProblemCategory(ProblemSource.AtCoder, ContestCategory.Beginner, 'C');
            await db.SaveChangesAsync();
        }

        var updateResponse = await _client.PatchAsJsonAsync(
            $"/study-records/{id}",
            new { durationSeconds = 601 }
        );
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(600, updated.GetProperty("targetSeconds").GetInt32());
        var detail = await _client.GetFromJsonAsync<JsonElement>($"/learning-items/{itemId}");
        Assert.Equal("needs_review", detail.GetProperty("status").GetString());

        var history = await _client.GetFromJsonAsync<JsonElement>(
            $"/learning-items/{itemId}/study-records"
        );
        Assert.Equal(600, history[0].GetProperty("targetSeconds").GetInt32());

        var newResponse = await _client.PostAsJsonAsync(
            $"/learning-items/{itemId}/study-records",
            new
            {
                result = "success",
                studiedAt = "2026-11-02T00:00:00Z",
                durationSeconds = 601,
            }
        );
        Assert.Equal(HttpStatusCode.Created, newResponse.StatusCode);
        var newRecord = await newResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1200, newRecord.GetProperty("targetSeconds").GetInt32());
        Assert.Equal("completed", newRecord.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("""{"result":"reviewed","studiedAt":"2026-11-01T00:00:00Z","durationSeconds":0}""")]
    [InlineData("""{"result":"success","studiedAt":"2026-11-01T00:00:00Z"}""")]
    [InlineData(
        """{"result":"success","studiedAt":"2026-11-01T00:00:00Z","durationSeconds":null}"""
    )]
    public async Task 廃止した結果と所要時間なしの記録を拒否する(string json)
    {
        var itemResponse = await _client.PostAsJsonAsync(
            "/learning-items",
            new
            {
                itemType = "algorithm_problem",
                title = "問題",
                source = "AtCoder",
                category = "Beginner",
                problemIndex = "A",
            }
        );
        Assert.Equal(HttpStatusCode.Created, itemResponse.StatusCode);
        var item = await itemResponse.Content.ReadFromJsonAsync<JsonElement>();
        int itemId = item.GetProperty("id").GetInt32();

        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"/learning-items/{itemId}/study-records", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var history = await _client.GetFromJsonAsync<JsonElement>(
            $"/learning-items/{itemId}/study-records"
        );
        Assert.Empty(history.EnumerateArray());
    }

    [Theory]
    [InlineData("AtCoder", "Beginner", "H")]
    [InlineData("OnlineMathContest", "Grand", "A")]
    [InlineData("0", "Beginner", "A")]
    [InlineData("AtCoder", "Beginner", "a")]
    public async Task 未対応の区分では問題を登録しない(
        string source,
        string category,
        string problemIndex
    )
    {
        var response = await _client.PostAsJsonAsync(
            "/learning-items",
            new
            {
                itemType = "algorithm_problem",
                title = "問題",
                source,
                category,
                problemIndex,
            }
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var items = await _client.GetFromJsonAsync<JsonElement>("/learning-items");
        Assert.Empty(items.EnumerateArray());
    }
}
