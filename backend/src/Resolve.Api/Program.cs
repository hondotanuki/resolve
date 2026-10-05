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

app.MapPost(
    "/learning-items",
    async (
        CreateLearningItemRequest request,
        CreateLearningItemUseCase useCase,
        CancellationToken cancellationToken
    ) =>
    {
        // itemType文字列 -> enum
        if (!ItemTypeMapping.TryParse(request.ItemType, out var itemType))
        {
            return Results.BadRequest();
        }

        var input = new CreateLearningItemInput
        {
            ItemType = itemType,
            Title = request.Title,
            Content = request.Content,
        };

        try
        {
            var output = await useCase.ExecuteAsync(input, cancellationToken);

            var response = new CreateLearningItemResponse
            {
                Id = output.Id,
                ItemType = ItemTypeMapping.ToApiValue(output.ItemType),
                Title = output.Title,
                ArchivedAt = output.ArchivedAt,
            };

            return Results.Created($"/learning-items/{response.Id}", response);
        }
        catch (ArgumentException)
        {
            return Results.BadRequest();
        }
    }
);

app.MapGet(
    "/learning-items",
    async (ListLearningItemsUseCase useCase, CancellationToken cancellationToken) =>
    {
        var output = await useCase.ExecuteAsync(cancellationToken);

        var response = output
            .Select(x => new ListLearningItemsResponse
            {
                Id = x.Id,
                ItemType = ItemTypeMapping.ToApiValue(x.ItemType),
                Title = x.Title,
                ArchivedAt = x.ArchivedAt,
            })
            .ToList();

        return Results.Ok(response);
    }
);

app.MapGet(
    "/learning-items/{id:int}",
    async (int id, GetLearningItemByIdUseCase useCase, CancellationToken cancellationToken) =>
    {
        var output = await useCase.ExecuteAsync(id, cancellationToken);

        if (output is null)
        {
            return Results.NotFound();
        }

        var response = new GetLearningItemResponse
        {
            Id = output.Id,
            ItemType = ItemTypeMapping.ToApiValue(output.ItemType),
            Title = output.Title,
            Content = output.Content,
            ArchivedAt = output.ArchivedAt,
            CreatedAt = output.CreatedAt,
            UpdatedAt = output.UpdatedAt,
        };

        return Results.Ok(response);
    }
);

app.Run();
