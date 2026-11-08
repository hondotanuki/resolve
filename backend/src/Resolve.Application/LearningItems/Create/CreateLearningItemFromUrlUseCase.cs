using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.LearningItems.Create;

public sealed record CreateLearningItemFromUrlOutput(int Id, bool Created);

public sealed class CreateLearningItemFromUrlUseCase(
    ILearningItemRepository repository,
    IProblemMetadataProvider metadataProvider
)
{
    public async Task<CreateLearningItemFromUrlOutput> ExecuteAsync(
        Uri input,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = metadataProvider.Normalize(input);
        var existing = await repository.FindByProblemUrlAsync(
            normalized.AbsoluteUri,
            cancellationToken
        );
        if (existing is not null)
            return new(existing.Id, false);
        var metadata = await metadataProvider.GetAsync(normalized, cancellationToken);
        if (metadata.ProblemUrl != normalized)
            throw new InvalidOperationException(
                "The problem metadata URL does not match the requested URL."
            );
        var item = new LearningItem(
            metadata.Title,
            metadata.Source,
            metadata.Category,
            metadata.ProblemIndex,
            normalized.AbsoluteUri,
            metadata.ContestId,
            metadata.ContestName,
            metadata.ContestStartDate
        );
        var result = await repository.CreateOrGetByProblemUrlAsync(item, cancellationToken);
        return new(result.Id, ReferenceEquals(result, item));
    }
}
