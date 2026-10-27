using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resolve.Domain;
using Resolve.Infrastructure.Persistence;
using Resolve.Infrastructure.Persistence.Repositories;

namespace Resolve.Infrastructure.Tests;

public sealed class EfStudyRecordRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ResolveDbContext _dbContext;
    private readonly EfStudyRecordRepository _repository;

    private static readonly DateTime StudiedAt = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    public EfStudyRecordRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ResolveDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ResolveDbContext(options);
        _dbContext.Database.Migrate();
        _repository = new EfStudyRecordRepository(_dbContext);
    }

    private async Task<LearningItem> CreateLearningItemAsync(string title = "二分探索")
    {
        var item = new LearningItem(ItemType.AlgorithmProblem, title, null);

        _dbContext.LearningItems.Add(item);
        await _dbContext.SaveChangesAsync();

        return item;
    }

    [Fact]
    public async Task CreateAsyncで保存して再取得できる()
    {
        // Arrange
        var item = await CreateLearningItemAsync();

        var record = new StudyRecord(
            item.Id,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "二分探索を学習した"
        );

        // Act
        var saved = await _repository.CreateAsync(record);

        // Assert: 保存後にIDが採番される
        Assert.True(saved.Id > 0);

        // DBから改めて読み直す
        _dbContext.ChangeTracker.Clear();

        var actual = await _dbContext.StudyRecords.SingleAsync(x => x.Id == saved.Id);

        Assert.Equal(item.Id, actual.LearningItemId);
        Assert.Equal(StudyResult.Success, actual.Result);
        Assert.Equal(StudiedAt, actual.StudiedAt);
        Assert.Equal(1800, actual.DurationSeconds);
        Assert.Equal("二分探索を学習した", actual.Memo);
        Assert.Equal(record.CreatedAt, actual.CreatedAt);
        Assert.Equal(record.UpdatedAt, actual.UpdatedAt);
    }

    [Fact]
    public async Task DurationSecondsとMemoはnullで保存できる()
    {
        var item = await CreateLearningItemAsync();
        var record = new StudyRecord(item.Id, StudyResult.Reviewed, StudiedAt);
        var saved = await _repository.CreateAsync(record);

        _dbContext.ChangeTracker.Clear();
        var actual = await _dbContext.StudyRecords.SingleAsync(x => x.Id == saved.Id);

        Assert.Null(actual.DurationSeconds);
        Assert.Null(actual.Memo);
        Assert.Equal(StudyResult.Reviewed, actual.Result);
    }

    [Fact]
    public async Task StudiedAtが最新の記録を取得できる()
    {
        var item = await CreateLearningItemAsync();
        var otherItem = await CreateLearningItemAsync("クイックソート");

        // 先に新しい学習日時の記録を登録
        await _repository.CreateAsync(new StudyRecord(item.Id, StudyResult.Failure, StudiedAt));

        // 後から古い学習日時の記録を登録
        await _repository.CreateAsync(
            new StudyRecord(item.Id, StudyResult.Success, StudiedAt.AddDays(-3))
        );

        // 別のLearningItemの、さらに新しい記録
        await _repository.CreateAsync(
            new StudyRecord(otherItem.Id, StudyResult.Reviewed, StudiedAt.AddDays(1))
        );

        _dbContext.ChangeTracker.Clear();

        // Act
        var latest = await _repository.GetLatestByLearningItemIdAsync(item.Id);

        // Assert
        Assert.NotNull(latest);
        Assert.Equal(item.Id, latest.LearningItemId);
        Assert.Equal(StudyResult.Failure, latest.Result);
        Assert.Equal(StudiedAt, latest.StudiedAt);
    }

    [Fact]
    public async Task 学習記録がなければnullを返す()
    {
        var item = await CreateLearningItemAsync();
        var latest = await _repository.GetLatestByLearningItemIdAsync(item.Id);

        Assert.Null(latest);
    }

    [Fact]
    public async Task 存在しないLearningItemIdでは保存できない()
    {
        var record = new StudyRecord(999, StudyResult.Success, StudiedAt);

        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(record));
    }

    [Fact]
    public async Task StudyResultは文字列として保存される()
    {
        var item = await CreateLearningItemAsync();
        var saved = await _repository.CreateAsync(
            new StudyRecord(item.Id, StudyResult.Failure, StudiedAt)
        );

        using var command = _connection.CreateCommand();

        command.CommandText = "SELECT result FROM study_records WHERE id = $id";
        command.Parameters.AddWithValue("$id", saved.Id);

        var value = await command.ExecuteScalarAsync();

        Assert.Equal("Failure", value);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
