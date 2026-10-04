using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resolve.Infrastructure;

namespace Resolve.Api.Tests;

public sealed class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. 本来のDbContext設定を削除
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ResolveDbContext>)
            );

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // 2. SQLite In-Memoryを作る
            _connection = new SqliteConnection("DataSource=:memory:");

            // 3. 接続を開いたままにする
            _connection.Open();

            // 4. ResolveDbContextをSQLiteへ差し替える
            services.AddDbContext<ResolveDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // DIからテスト用DbContextを取得
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ResolveDbContext>();

        // Migrationを適用
        dbContext.Database.Migrate();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
