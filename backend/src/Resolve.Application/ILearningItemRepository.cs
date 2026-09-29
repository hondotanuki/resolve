using Resolve.Domain;

namespace Resolve.Application;

public interface ILearningItemRepository
{
    Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    );
}
