using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Resolve.Infrastructure.Persistence;

namespace Resolve.Infrastructure.Tests;

public sealed class ProblemCategoryMigrationTests
{
    [Fact]
    public async Task 問題URLとコンテスト情報の追加時に既存の問題を保持する()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ResolveDbContext>().UseSqlite(connection).Options;
        await using var context = new ResolveDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261108074817_RemoveLearningItemType");
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO learning_items (title, source, category, problem_index, created_at, updated_at) VALUES ('既存の問題', 'AtCoder', 'Beginner', 'A', '2026-01-01', '2026-01-01')"
        );

        await migrator.MigrateAsync();

        var item = await context.LearningItems.SingleAsync();
        Assert.Equal("既存の問題", item.Title);
        Assert.Equal('A', item.ProblemIndex);
        Assert.Null(item.ProblemUrl);
        Assert.Null(item.ContestId);
        Assert.Null(item.ContestName);
        Assert.Null(item.ContestStartDate);
    }

    [Fact]
    public async Task ContentとItemTypeの削除後も問題と学習記録を保持する()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ResolveDbContext>().UseSqlite(connection).Options;
        await using var context = new ResolveDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261108024947_AddProblemCategoryAndStudyTarget");
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO learning_items (item_type, title, content, source, category, problem_index, created_at, updated_at) VALUES ('AlgorithmProblem', '問題', '削除対象', 'AtCoder', 'Beginner', 'A', '2026-01-01', '2026-01-01')"
        );
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO study_records (learning_item_id, result, studied_at, duration_seconds, target_seconds, memo, created_at, updated_at) VALUES (1, 'Success', '2026-01-01', 30, 600, '復習', '2026-01-01', '2026-01-01')"
        );

        await migrator.MigrateAsync();

        var item = await context.LearningItems.SingleAsync();
        Assert.Equal("問題", item.Title);
        Assert.Equal('A', item.ProblemIndex);
        var record = await context.StudyRecords.SingleAsync();
        Assert.Equal("復習", record.Memo);
        Assert.Equal(600, record.TargetSeconds);
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM pragma_table_info('learning_items') WHERE name = 'content'";
        Assert.Equal(0L, await command.ExecuteScalarAsync());
        command.CommandText =
            "SELECT COUNT(*) FROM pragma_table_info('learning_items') WHERE name = 'item_type'";
        Assert.Equal(0L, await command.ExecuteScalarAsync());
    }

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
            "INSERT INTO learning_items (title, source, category, problem_index, created_at, updated_at) VALUES ('問題', 'AtCoder', 'Beginner', 'A', '2026-01-01', '2026-01-01')"
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
