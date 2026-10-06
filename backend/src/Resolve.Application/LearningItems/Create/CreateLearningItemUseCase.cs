using Resolve.Domain;

namespace Resolve.Application;

public sealed class CreateLearningItemUseCase
{
    private readonly ILearningItemRepository _repository;

    public CreateLearningItemUseCase(ILearningItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateLearningItemOutput> ExecuteAsync(
        CreateLearningItemInput input,
        CancellationToken cancellationToken = default
    )
    {
        var learningItem = new LearningItem(input.ItemType, input.Title, input.Content);
        var saved = await _repository.CreateAsync(learningItem, cancellationToken);
        return new CreateLearningItemOutput(
            saved.Id,
            saved.ItemType,
            saved.Title,
            saved.Content,
            saved.ArchivedAt,
            saved.CreatedAt,
            saved.UpdatedAt
        );
    }
}
