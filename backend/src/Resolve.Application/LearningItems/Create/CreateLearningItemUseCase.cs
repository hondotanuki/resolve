using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.LearningItems.Create;

public sealed class CreateLearningItemUseCase(
    ILearningItemRepository repository,
    IProblemMetadataProvider metadataProvider
)
{
    public async Task<CreateLearningItemOutput> ExecuteAsync(
        CreateLearningItemInput input,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = metadataProvider.Normalize(input.Url);
        var existing = await repository.FindByProblemUrlAsync(
            normalized.AbsoluteUri,
            cancellationToken
        );
        if (existing is not null)
            return new(existing.Id, false);
        var metadata = await metadataProvider.GetAsync(normalized, cancellationToken);
        if (metadata.ProblemUrl != normalized)
            throw new InvalidDataException("取得した問題情報のURLが指定されたURLと一致しません。");
        LearningItem item;
        try
        {
            item = new LearningItem(
                metadata.Title,
                metadata.Source,
                metadata.Category,
                metadata.ProblemIndex,
                normalized.AbsoluteUri,
                metadata.ContestId,
                metadata.ContestName,
                metadata.ContestStartDate
            );
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException(
                "外部サービスから取得した問題情報が不正です。",
                exception
            );
        }
        var result = await repository.CreateOrGetByProblemUrlAsync(item, cancellationToken);
        return new(result.Id, ReferenceEquals(result, item));
    }
}
