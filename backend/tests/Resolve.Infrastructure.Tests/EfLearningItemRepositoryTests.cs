using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resolve.Domain;
using Resolve.Infrastructure.Persistence;
using Resolve.Infrastructure.Persistence.Repositories;

namespace Resolve.Infrastructure.Tests;

public class EfLearningItemRepositoryTests
{
    [Fact]
    public async Task CreateAsyncでLearningItemを保存できる()
    {
        // 1. SQLiteインメモリDBを用意
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        // 2. DbContextの設定を作成
        var options = new DbContextOptionsBuilder<ResolveDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new ResolveDbContext(options);

        // 3. テーブルを作成
        await dbContext.Database.EnsureCreatedAsync();

        // 4. Repositoryを作成
        var repository = new EfLearningItemRepository(dbContext);

        // 5. LearningItemを保存
        var learningItem = new LearningItem(
            ItemType.AlgorithmProblem,
            "二分探索",
            "二分探索の問題を解く"
        );

        var saved = await repository.CreateAsync(learningItem);

        // 6. DBによってIdが採番されたことを確認
        Assert.True(saved.Id > 0);

        // 7. DbContextから取得し直す
        var actual = await dbContext
            .LearningItems.AsNoTracking()
            .SingleAsync(x => x.Id == saved.Id);

        // 8. 保存内容を確認
        Assert.Equal("二分探索", actual.Title);
        Assert.Equal(ItemType.AlgorithmProblem, actual.ItemType);
        Assert.Equal("二分探索の問題を解く", actual.Content);
        Assert.Equal(saved.CreatedAt, actual.CreatedAt);
        Assert.Equal(saved.UpdatedAt, actual.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, actual.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, actual.UpdatedAt.Kind);
        Assert.Equal(saved.ArchivedAt, actual.ArchivedAt);
    }

    [Fact]
    public async Task ArchivedAtをUTCで再取得できる()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ResolveDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new ResolveDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var item = new LearningItem(ItemType.AlgorithmProblem, "二分探索", null);
        dbContext.LearningItems.Add(item);
        var archivedAt = new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
        dbContext.Entry(item).Property(x => x.ArchivedAt).CurrentValue = archivedAt;
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.LearningItems.SingleAsync(x => x.Id == item.Id);

        Assert.NotNull(actual.ArchivedAt);
        Assert.Equal(archivedAt, actual.ArchivedAt.Value);
        Assert.Equal(DateTimeKind.Utc, actual.ArchivedAt.Value.Kind);
    }
}
