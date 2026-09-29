namespace Resolve.Infrastructure.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resolve.Domain;
using Resolve.Infrastructure.Persistence.Repositories;

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
        Assert.Equal(saved.ArchivedAt, actual.ArchivedAt);
    }
}
