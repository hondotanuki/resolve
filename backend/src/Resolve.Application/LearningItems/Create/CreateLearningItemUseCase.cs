using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.LearningItems.Create;

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
        var learningItem = new LearningItem(
            input.Title,
            input.Source,
            input.Category,
            input.ProblemIndex
        );
        var saved = await _repository.CreateAsync(learningItem, cancellationToken);
        return new CreateLearningItemOutput(
            saved.Id,
            saved.Title,
            saved.ArchivedAt,
            saved.CreatedAt,
            saved.UpdatedAt,
            saved.Source,
            saved.Category,
            saved.ProblemIndex
        );
    }
}
