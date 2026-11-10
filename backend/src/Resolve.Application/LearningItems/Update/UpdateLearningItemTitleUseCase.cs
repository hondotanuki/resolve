using Resolve.Application.Interfaces;

namespace Resolve.Application.LearningItems.Update;

public sealed class UpdateLearningItemTitleUseCase(ILearningItemRepository repository)
{
    public async Task<UpdateLearningItemTitleOutput> ExecuteAsync(
        UpdateLearningItemTitleInput input,
        CancellationToken cancellationToken = default
    )
    {
        var item = await repository.GetForUpdateAsync(input.Id, cancellationToken);
        if (item is null)
            return new(false);
        item.UpdateTitle(input.Title);
        await repository.SaveChangesAsync(cancellationToken);
        return new(true);
    }
}
