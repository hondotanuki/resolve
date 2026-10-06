using Resolve.Domain;

namespace Resolve.Application.Tests.Fakes;

public sealed class FakeLearningItemRepository : ILearningItemRepository
{
    public int CreateCallCount { get; private set; }
    public LearningItem? ReceivedItem { get; private set; }
    public LearningItem? CreateResult { get; set; }
    public IReadOnlyList<LearningItem> ListResult { get; set; } = [];
    public int? ReceivedId { get; private set; }
    public LearningItem? GetByIdResult { get; set; }

    public Task<LearningItem> CreateAsync(
        LearningItem learningItem,
        CancellationToken cancellationToken = default
    )
    {
        CreateCallCount++;
        ReceivedItem = learningItem;

        return Task.FromResult(CreateResult ?? learningItem);
    }

    public Task<IReadOnlyList<LearningItem>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(ListResult);
    }

    public Task<LearningItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ReceivedId = id;

        return Task.FromResult(GetByIdResult);
    }
}
