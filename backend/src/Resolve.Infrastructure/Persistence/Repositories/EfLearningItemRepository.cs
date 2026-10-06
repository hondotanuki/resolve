using Microsoft.EntityFrameworkCore;
using Resolve.Application;
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
}
