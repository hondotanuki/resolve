using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Resolve.Api.Contracts;
using Resolve.Api.LearningItems;
using Resolve.Api.StudyRecords;
using Resolve.Application.Interfaces;
using Resolve.Application.LearningItems.Create;
using Resolve.Application.LearningItems.GetById;
using Resolve.Application.LearningItems.List;
using Resolve.Application.LearningItems.Update;
using Resolve.Application.StudyRecords.Create;
using Resolve.Application.StudyRecords.Delete;
using Resolve.Application.StudyRecords.List;
using Resolve.Application.StudyRecords.Update;
using Resolve.Infrastructure.External;
using Resolve.Infrastructure.External.AtCoder;
using Resolve.Infrastructure.External.Http;
using Resolve.Infrastructure.External.OMC;
using Resolve.Infrastructure.Persistence;
using Resolve.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<AllowedValuesSchemaTransformer>();
    options.AddSchemaTransformer<UpdateStudyRecordSchemaTransformer>();
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
});
builder.Services.AddDbContext<ResolveDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.AddSingleton<IExternalRequestClock>(new ExternalRequestClock(TimeProvider.System));
builder.Services.AddSingleton(serviceProvider => new ExternalHttpClient(
    serviceProvider.GetRequiredService<IExternalRequestClock>(),
    new HttpClientHandler { AllowAutoRedirect = false }
));
builder.Services.AddSingleton(serviceProvider => new AtCoderDatasetCache(
    serviceProvider.GetRequiredService<ExternalHttpClient>(),
    serviceProvider.GetRequiredService<IExternalRequestClock>(),
    Path.Combine(builder.Environment.ContentRootPath, ".cache", "atcoder-problems", "datasets.json")
));

// External
builder.Services.AddSingleton<AtCoderProblemInfoProvider>();
builder.Services.AddSingleton<OMCProblemInfoProvider>();
builder.Services.AddSingleton<IProblemMetadataProvider, ProblemMetadataProvider>();

// Repositories
builder.Services.AddScoped<ILearningItemRepository, EfLearningItemRepository>();
builder.Services.AddScoped<IStudyRecordRepository, EfStudyRecordRepository>();

// UseCases
builder.Services.AddScoped<CreateLearningItemUseCase>();
builder.Services.AddScoped<ListLearningItemsUseCase>();
builder.Services.AddScoped<UpdateLearningItemTitleUseCase>();
builder.Services.AddScoped<GetLearningItemByIdUseCase>();
builder.Services.AddScoped<CreateStudyRecordUseCase>();
builder.Services.AddScoped<ListStudyRecordsUseCase>();
builder.Services.AddScoped<DeleteStudyRecordUseCase>();
builder.Services.AddScoped<UpdateStudyRecordUseCase>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.MapLearningItemEndpoints();
app.MapStudyRecordEndpoints();

app.Run();
