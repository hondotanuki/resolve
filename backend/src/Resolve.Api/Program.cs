using Microsoft.EntityFrameworkCore;
using Resolve.Api;
using Resolve.Application;
using Resolve.Infrastructure;
using Resolve.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ResolveDbContext>(options =>
{
    options.UseSqlite(connectionString);
});
builder.Services.AddScoped<ILearningItemRepository, EfLearningItemRepository>();
builder.Services.AddScoped<CreateLearningItemUseCase>();
builder.Services.AddScoped<ListLearningItemsUseCase>();
builder.Services.AddScoped<GetLearningItemByIdUseCase>();

var app = builder.Build();
app.MapLearningItemEndpoints();

app.Run();
