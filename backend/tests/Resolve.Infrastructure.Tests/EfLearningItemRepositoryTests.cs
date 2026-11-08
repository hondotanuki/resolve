using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resolve.Domain;
using Resolve.Infrastructure.Persistence;
using Resolve.Infrastructure.Persistence.Repositories;

namespace Resolve.Infrastructure.Tests;

public sealed class EfLearningItemRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ResolveDbContext _dbContext;
    private readonly EfLearningItemRepository _repository;

    public EfLearningItemRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ResolveDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ResolveDbContext(options);
        _dbContext.Database.Migrate();
        _repository = new EfLearningItemRepository(_dbContext);
    }

    [Fact]
    public async Task CreateAsyncでLearningItemを保存できる()
    {
        var item = new LearningItem(
            "二分探索",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

        var saved = await _repository.CreateAsync(item);

        var actual = await _dbContext
            .LearningItems.AsNoTracking()
            .SingleAsync(x => x.Id == saved.Id);

        Assert.True(saved.Id > 0);
        Assert.Equal("二分探索", actual.Title);

        Assert.Equal(saved.CreatedAt, actual.CreatedAt);
        Assert.Equal(saved.UpdatedAt, actual.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, actual.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, actual.UpdatedAt.Kind);
        Assert.Equal(saved.ArchivedAt, actual.ArchivedAt);
    }

    [Fact]
    public async Task 問題URLとコンテスト情報を保存し再取得できる()
    {
        var item = new LearningItem(
            "Nigiri",
            ProblemSource.AtCoder,
            ContestCategory.Beginner,
            'A',
            "https://atcoder.jp/contests/abc482/tasks/abc482_a",
            "abc482",
            "AtCoder Beginner Contest 482",
            new DateOnly(2026, 10, 31)
        );

        var saved = await _repository.CreateAsync(item);
        _dbContext.ChangeTracker.Clear();
        var actual = await _dbContext.LearningItems.SingleAsync(x => x.Id == saved.Id);

        Assert.Equal(item.ProblemUrl, actual.ProblemUrl);
        Assert.Equal(item.ContestId, actual.ContestId);
        Assert.Equal(item.ContestName, actual.ContestName);
        Assert.Equal(item.ContestStartDate, actual.ContestStartDate);
    }

    [Fact]
    public async Task ArchivedAtをUTCで再取得できる()
    {
        var item = new LearningItem(
            "二分探索",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );
        _dbContext.LearningItems.Add(item);
        var archivedAt = new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
        _dbContext.Entry(item).Property(x => x.ArchivedAt).CurrentValue = archivedAt;
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var actual = await _dbContext.LearningItems.SingleAsync(x => x.Id == item.Id);

        Assert.NotNull(actual.ArchivedAt);
        Assert.Equal(archivedAt, actual.ArchivedAt.Value);
        Assert.Equal(DateTimeKind.Utc, actual.ArchivedAt.Value.Kind);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
