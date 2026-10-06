using Resolve.Application.Interfaces;

namespace Resolve.Application.LearningItems.GetById;

public sealed class GetLearningItemByIdUseCase
{
    private readonly ILearningItemRepository _repository;

    public GetLearningItemByIdUseCase(ILearningItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetLearningItemOutput?> ExecuteAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return null;
        }

        return new GetLearningItemOutput(
            item.Id,
            item.ItemType,
            item.Title,
            item.Content,
            item.ArchivedAt,
            item.CreatedAt,
            item.UpdatedAt
        );
    }
}
