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
        var item = new LearningItem(
            ItemType.AlgorithmProblem,
            title,
            null,
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

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
            memo: "二分探索を学習した",
            targetSeconds: 600
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
        Assert.Equal(DateTimeKind.Utc, actual.StudiedAt.Kind);
        Assert.Equal(1800, actual.DurationSeconds);
        Assert.Equal("二分探索を学習した", actual.Memo);
        Assert.Equal(record.CreatedAt, actual.CreatedAt);
        Assert.Equal(record.UpdatedAt, actual.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, actual.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, actual.UpdatedAt.Kind);
    }

    [Fact]
    public async Task DurationSecondsとMemoはnullで保存できる()
    {
        var item = await CreateLearningItemAsync();
        var record = new StudyRecord(
            item.Id,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 0,
            targetSeconds: 600
        );
        var saved = await _repository.CreateAsync(record);

        _dbContext.ChangeTracker.Clear();
        var actual = await _dbContext.StudyRecords.SingleAsync(x => x.Id == saved.Id);

        Assert.Equal(0, actual.DurationSeconds);
        Assert.Null(actual.Memo);
        Assert.Equal(StudyResult.Success, actual.Result);
    }

    [Fact]
    public async Task StudiedAtが最新の記録を取得できる()
    {
        var item = await CreateLearningItemAsync();
        var otherItem = await CreateLearningItemAsync("クイックソート");

        // 先に新しい学習日時の記録を登録
        await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Failure,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );

        // 後から古い学習日時の記録を登録
        await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Success,
                StudiedAt.AddDays(-3),
                durationSeconds: 0,
                targetSeconds: 600
            )
        );

        // 別のLearningItemの、さらに新しい記録
        await _repository.CreateAsync(
            new StudyRecord(
                otherItem.Id,
                StudyResult.Success,
                StudiedAt.AddDays(1),
                durationSeconds: 0,
                targetSeconds: 600
            )
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
        var record = new StudyRecord(
            999,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 0,
            targetSeconds: 600
        );

        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.CreateAsync(record));
    }

    [Fact]
    public async Task StudyResultは文字列として保存される()
    {
        var item = await CreateLearningItemAsync();
        var saved = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Failure,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );

        using var command = _connection.CreateCommand();

        command.CommandText = "SELECT result FROM study_records WHERE id = $id";
        command.Parameters.AddWithValue("$id", saved.Id);

        var value = await command.ExecuteScalarAsync();

        Assert.Equal("Failure", value);
    }

    [Fact]
    public async Task StudiedAtが同じならIDの大きい記録を取得する()
    {
        var item = await CreateLearningItemAsync();

        var first = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Failure,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        var second = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Success,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        _dbContext.ChangeTracker.Clear();

        var latest = await _repository.GetLatestByLearningItemIdAsync(item.Id);

        Assert.True(second.Id > first.Id);
        Assert.NotNull(latest);
        Assert.Equal(second.Id, latest.Id);
        Assert.Equal(item.Id, latest.LearningItemId);
        Assert.Equal(StudyResult.Success, latest.Result);
        Assert.Equal(StudiedAt, latest.StudiedAt);
    }

    [Fact]
    public async Task StudiedAtが同じならIDの大きい記録から状態を算出する()
    {
        var item = await CreateLearningItemAsync();

        var first = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Failure,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        var second = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Success,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );

        Assert.True(second.Id > first.Id);

        // IDの小さい順に渡し、入力順ではなくIDで選ばれることを確認する。
        var status = ItemStatusCalculator.Calculate([first, second]);

        Assert.Equal(ItemStatus.Completed, status);
    }

    [Fact]
    public async Task ListByLearningItemIdAsyncは対象の記録を日時とIDの降順で返す()
    {
        var item = await CreateLearningItemAsync();
        var otherItem = await CreateLearningItemAsync("別の学習対象");

        var sameTimeFirst = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Failure,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        var newer = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Success,
                StudiedAt.AddDays(1),
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        var sameTimeSecond = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Success,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        var older = await _repository.CreateAsync(
            new StudyRecord(
                item.Id,
                StudyResult.Success,
                StudiedAt.AddDays(-1),
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        await _repository.CreateAsync(
            new StudyRecord(
                otherItem.Id,
                StudyResult.Success,
                StudiedAt.AddDays(2),
                durationSeconds: 0,
                targetSeconds: 600
            )
        );
        _dbContext.ChangeTracker.Clear();

        var actual = await _repository.ListByLearningItemIdAsync(item.Id);

        Assert.Equal(
            new[] { newer.Id, sameTimeSecond.Id, sameTimeFirst.Id, older.Id },
            actual.Select(record => record.Id)
        );
        Assert.All(actual, record => Assert.Equal(item.Id, record.LearningItemId));
    }

    [Fact]
    public async Task ListByLearningItemIdAsyncは対象の記録がなければ空の一覧を返す()
    {
        var item = await CreateLearningItemAsync();
        var otherItem = await CreateLearningItemAsync("別の学習対象");
        await _repository.CreateAsync(
            new StudyRecord(
                otherItem.Id,
                StudyResult.Success,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            )
        );

        var actual = await _repository.ListByLearningItemIdAsync(item.Id);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task 訂正した学習記録をDBに保存できる()
    {
        // Arrange
        var created = await CreateStudyRecordAsync();

        var originalCreatedAt = created.CreatedAt;
        var studyRecordId = created.Id;
        var learningItemId = created.LearningItemId;

        _dbContext.ChangeTracker.Clear();

        var record = await _repository.GetByIdAsync(studyRecordId);
        Assert.NotNull(record);

        var updatedStudiedAt = StudiedAt.AddDays(1);

        // Act
        var before = DateTime.UtcNow;

        record.Update(StudyResult.Success, updatedStudiedAt, durationSeconds: 600, memo: "訂正後");

        await _repository.UpdateAsync(record);

        var after = DateTime.UtcNow;

        // DBから再取得
        _dbContext.ChangeTracker.Clear();

        var actual = await _dbContext
            .StudyRecords.AsNoTracking()
            .SingleAsync(x => x.Id == studyRecordId);

        // Assert
        Assert.Equal(studyRecordId, actual.Id);
        Assert.Equal(learningItemId, actual.LearningItemId);
        Assert.Equal(updatedStudiedAt, actual.StudiedAt);
        Assert.Equal(StudyResult.Success, actual.Result);
        Assert.Equal(600, actual.DurationSeconds);
        Assert.Equal("訂正後", actual.Memo);
        Assert.Equal(originalCreatedAt, actual.CreatedAt);
        Assert.InRange(actual.UpdatedAt, before, after);
        Assert.Equal(DateTimeKind.Utc, actual.StudiedAt.Kind);
    }

    [Fact]
    public async Task 学習記録をDBから削除できる()
    {
        // Arrange
        var created = await CreateStudyRecordAsync();
        var studyRecordId = created.Id;

        _dbContext.ChangeTracker.Clear();

        var record = await _repository.GetByIdAsync(studyRecordId);
        Assert.NotNull(record);

        // Act
        await _repository.DeleteAsync(record);

        _dbContext.ChangeTracker.Clear();

        var actual = await _dbContext
            .StudyRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == studyRecordId);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task 存在しないIDならnullを返す()
    {
        // Act
        var actual = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(actual);
    }

    private async Task<StudyRecord> CreateStudyRecordAsync()
    {
        var learningItem = new LearningItem(
            ItemType.AlgorithmProblem,
            "二分探索",
            null,
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

        _dbContext.LearningItems.Add(learningItem);
        await _dbContext.SaveChangesAsync();

        var record = new StudyRecord(
            learningItem.Id,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前",
            targetSeconds: 600
        );

        return await _repository.CreateAsync(record);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
