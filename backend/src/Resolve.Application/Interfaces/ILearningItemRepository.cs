using Resolve.Domain;

namespace Resolve.Application;

public interface ILearningItemRepository
{
    Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<LearningItem>> ListAsync(CancellationToken cancellationToken = default);

    Task<LearningItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
