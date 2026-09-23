using Microsoft.EntityFrameworkCore;
using Resolve.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ResolveDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

var app = builder.Build();

app.Run();
