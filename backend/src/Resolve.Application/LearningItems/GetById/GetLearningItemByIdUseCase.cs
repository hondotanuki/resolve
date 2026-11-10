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

    public async Task<GetLearningItemByIdOutput?> ExecuteAsync(
        GetLearningItemByIdInput input,
        CancellationToken cancellationToken = default
    )
    {
        var item = await _learningItemRepository.GetByIdAsync(input.Id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        StudyRecord? latestRecord = await _studyRecordRepository.GetLatestByLearningItemIdAsync(
            input.Id,
            cancellationToken
        );
        StudyRecord[] latestRecords = latestRecord is null ? [] : [latestRecord];
        ItemStatus status = ItemStatusCalculator.Calculate(latestRecords);

        return new GetLearningItemByIdOutput(
            item.Id,
            item.Title,
            status,
            item.ArchivedAt,
            item.CreatedAt,
            item.UpdatedAt,
            item.Source,
            item.Category,
            item.ProblemIndex,
            item.ProblemUrl,
            item.ContestId,
            item.ContestName,
            item.ContestStartDate,
            TargetTime.GetSeconds(item.Source, item.Category, item.ProblemIndex)
                ?? throw new InvalidOperationException("問題区分に対応する目標時間がありません。")
        );
    }
}
