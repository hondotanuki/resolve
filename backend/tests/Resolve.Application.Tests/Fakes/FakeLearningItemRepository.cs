using Resolve.Application.Interfaces;
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
    public LearningItem? FindByUrlResult { get; set; }

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

    public Task<LearningItem?> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default
    ) => GetByIdAsync(id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<LearningItem?> FindByProblemUrlAsync(
        string problemUrl,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(FindByUrlResult);

    public Task<LearningItem> CreateOrGetByProblemUrlAsync(
        LearningItem item,
        CancellationToken cancellationToken = default
    ) => CreateAsync(item, cancellationToken);
}
