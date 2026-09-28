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
}
