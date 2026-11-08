using Resolve.Application.Interfaces;

namespace Resolve.Application.LearningItems.Update;

public sealed class UpdateLearningItemTitleUseCase(ILearningItemRepository repository)
{
    public async Task<bool> ExecuteAsync(
        int id,
        string title,
        CancellationToken cancellationToken = default
    )
    {
        var item = await repository.GetForUpdateAsync(id, cancellationToken);
        if (item is null)
            return false;
        item.UpdateTitle(title);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
