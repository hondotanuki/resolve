using Resolve.Domain;

namespace Resolve.Application.Interfaces;

public interface ILearningItemRepository
{
    Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<LearningItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<LearningItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LearningItem?> GetForUpdateAsync(int id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<LearningItem?> FindByProblemUrlAsync(
        string problemUrl,
        CancellationToken cancellationToken = default
    );
    Task<LearningItem> CreateOrGetByProblemUrlAsync(
        LearningItem item,
        CancellationToken cancellationToken = default
    );
}
