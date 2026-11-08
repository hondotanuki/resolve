using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Resolve.Infrastructure.Persistence;

namespace Resolve.Infrastructure.Tests;

public sealed class ProblemCategoryMigrationTests
{
    [Theory]
    [InlineData("source")]
    [InlineData("category")]
    [InlineData("problem_index")]
    public async Task 問題区分のDB列はnullを拒否する(string column)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ResolveDbContext>().UseSqlite(connection).Options;
        await using var context = new ResolveDbContext(options);
        await context.Database.MigrateAsync();
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO learning_items (item_type, title, source, category, problem_index, created_at, updated_at) VALUES ('AlgorithmProblem', '問題', 'AtCoder', 'Beginner', 'A', '2026-01-01', '2026-01-01')"
        );

        // 列名はInlineDataの固定値のみを使用する。
        await using var command = connection.CreateCommand();
        command.CommandText = $"UPDATE learning_items SET {column} = NULL";
        await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task 既存の問題があるDBには目標時間を推測して移行しない()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ResolveDbContext>().UseSqlite(connection).Options;
        await using var context = new ResolveDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261027133554_AddStudyRecords");
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO learning_items (item_type, title, created_at, updated_at) VALUES ('AlgorithmProblem', 'old', '2026-01-01', '2026-01-01')"
        );

        await Assert.ThrowsAsync<SqliteException>(() => migrator.MigrateAsync());

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM learning_items";
        Assert.Equal(1L, await command.ExecuteScalarAsync());
    }
}
