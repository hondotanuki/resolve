using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Create;

public sealed class CreateStudyRecordUseCase(
    ILearningItemRepository learningItemRepository,
    IStudyRecordRepository studyRecordRepository
)
{
    public async Task<CreateStudyRecordOutput?> ExecuteAsync(
        CreateStudyRecordInput input,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(input);

        // LearningItemの存在確認
        LearningItem? learningItem = await learningItemRepository.GetByIdAsync(
            input.LearningItemId,
            cancellationToken
        );

        if (learningItem is null)
        {
            return null;
        }

        if (
            TargetTime.GetSeconds(
                learningItem.Source,
                learningItem.Category,
                learningItem.ProblemIndex
            )
            is not { } targetSeconds
        )
            throw new ArgumentException("学習対象の問題区分に対応する目標時間がありません。");

        StudyRecord record = new(
            input.LearningItemId,
            input.Result,
            input.StudiedAt,
            input.DurationSeconds,
            targetSeconds,
            input.Memo
        );
        StudyRecord createdRecord = await studyRecordRepository.CreateAsync(
            record,
            cancellationToken
        );
        StudyRecord? latestRecord = await studyRecordRepository.GetLatestByLearningItemIdAsync(
            input.LearningItemId,
            cancellationToken
        );

        StudyRecord[] latestRecords = latestRecord is null ? [] : [latestRecord];
        ItemStatus status = ItemStatusCalculator.Calculate(latestRecords);

        return new CreateStudyRecordOutput(
            createdRecord.Id,
            createdRecord.LearningItemId,
            createdRecord.Result,
            createdRecord.StudiedAt,
            createdRecord.DurationSeconds,
            createdRecord.TargetSeconds,
            createdRecord.Memo,
            createdRecord.CreatedAt,
            createdRecord.UpdatedAt,
            status
        );
    }
}
