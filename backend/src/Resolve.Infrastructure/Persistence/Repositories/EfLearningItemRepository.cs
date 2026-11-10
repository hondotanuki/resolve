using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Infrastructure.Persistence.Repositories;

public sealed class EfLearningItemRepository : ILearningItemRepository
{
    private readonly ResolveDbContext _dbContext;

    public EfLearningItemRepository(ResolveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.LearningItems.Add(learningItem);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return learningItem;
    }

    public async Task<IReadOnlyList<LearningItem>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .LearningItems.AsNoTracking()
            .Where(x => x.ArchivedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<LearningItem?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .LearningItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<LearningItem?> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default
    ) => _dbContext.LearningItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public Task<LearningItem?> FindByProblemUrlAsync(
        string problemUrl,
        CancellationToken cancellationToken = default
    ) =>
        _dbContext
            .LearningItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProblemUrl == problemUrl, cancellationToken);

    public async Task<LearningItem> CreateOrGetByProblemUrlAsync(
        LearningItem item,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.LearningItems.Add(item);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return item;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException
                    is SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 }
            )
        {
            _dbContext.Entry(item).State = EntityState.Detached;
            var existing = await FindByProblemUrlAsync(item.ProblemUrl!, cancellationToken);
            if (existing is null)
                throw;
            return existing;
        }
    }
}
