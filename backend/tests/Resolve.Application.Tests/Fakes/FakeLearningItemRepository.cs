using Resolve.Domain;

namespace Resolve.Application.Tests.Fakes;

public sealed class FakeLearningItemRepository : ILearningItemRepository
{
    public int CreateCallCount { get; private set; }
    public LearningItem? ReceivedItem { get; private set; }
    public LearningItem? ItemToReturn { get; set; }
    public IReadOnlyList<LearningItem> ItemsToReturn { get; set; } = [];

    public Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    )
    {
        CreateCallCount++;
        ReceivedItem = learningItem;

        return Task.FromResult(ItemToReturn ?? learningItem);
    }

    public Task<IReadOnlyList<LearningItem>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(ItemsToReturn);
    }
}
