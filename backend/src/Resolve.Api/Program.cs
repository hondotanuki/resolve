using Microsoft.EntityFrameworkCore;
using Resolve.Api.LearningItems;
using Resolve.Api.StudyRecords;
using Resolve.Application.Interfaces;
using Resolve.Application.LearningItems.Create;
using Resolve.Application.LearningItems.GetById;
using Resolve.Application.LearningItems.List;
using Resolve.Application.StudyRecords.Create;
using Resolve.Infrastructure.Persistence;
using Resolve.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ResolveDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// Repositories
builder.Services.AddScoped<ILearningItemRepository, EfLearningItemRepository>();
builder.Services.AddScoped<IStudyRecordRepository, EfStudyRecordRepository>();

// UseCases
builder.Services.AddScoped<CreateLearningItemUseCase>();
builder.Services.AddScoped<ListLearningItemsUseCase>();
builder.Services.AddScoped<GetLearningItemByIdUseCase>();
builder.Services.AddScoped<CreateStudyRecordUseCase>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapLearningItemEndpoints();
app.MapStudyRecordEndpoints();

app.Run();
