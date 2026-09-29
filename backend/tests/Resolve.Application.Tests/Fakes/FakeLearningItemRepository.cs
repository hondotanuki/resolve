using Resolve.Domain;

namespace Resolve.Application.Tests.Fakes;

public sealed class FakeLearningItemRepository : ILearningItemRepository
{
    public int CreateCallCount { get; private set; }

    public LearningItem? ReceivedItem { get; private set; }

    public LearningItem? ItemToReturn { get; set; }

    public Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    )
    {
        CreateCallCount++;
        ReceivedItem = learningItem;

        return Task.FromResult(ItemToReturn ?? learningItem);
    }
}
