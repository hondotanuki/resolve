namespace Resolve.Application;

public sealed class ListLearningItemsUseCase
{
    private readonly ILearningItemRepository _repository;

    public ListLearningItemsUseCase(ILearningItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ListLearningItemsOutput>> ExecuteAsync(
        CancellationToken cancellationToken = default
    )
    {
        var items = await _repository.ListAsync(cancellationToken);

        return items
            .Select(x => new ListLearningItemsOutput(x.Id, x.ItemType, x.Title, x.ArchivedAt))
            .ToList();
    }
}
