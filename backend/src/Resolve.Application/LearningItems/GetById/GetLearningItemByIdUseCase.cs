using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.LearningItems.GetById;

public sealed class GetLearningItemByIdUseCase
{
    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IStudyRecordRepository _studyRecordRepository;

    public GetLearningItemByIdUseCase(
        ILearningItemRepository learningItemRepository,
        IStudyRecordRepository studyRecordRepository
    )
    {
        _learningItemRepository = learningItemRepository;
        _studyRecordRepository = studyRecordRepository;
    }

    public async Task<GetLearningItemOutput?> ExecuteAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var item = await _learningItemRepository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        StudyRecord? latestRecord = await _studyRecordRepository.GetLatestByLearningItemIdAsync(
            id,
            cancellationToken
        );
        StudyRecord[] latestRecords = latestRecord is null ? [] : [latestRecord];
        ItemStatus status = ItemStatusCalculator.Calculate(latestRecords);

        return new GetLearningItemOutput(
            item.Id,
            item.ItemType,
            item.Title,
            status,
            item.Content,
            item.ArchivedAt,
            item.CreatedAt,
            item.UpdatedAt,
            item.Source,
            item.Category,
            item.ProblemIndex
        );
    }
}
