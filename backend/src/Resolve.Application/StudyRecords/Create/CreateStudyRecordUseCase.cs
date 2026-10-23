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

        StudyRecord record = new(
            input.LearningItemId,
            input.Result,
            input.StudiedAt,
            input.DurationSeconds,
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
            createdRecord.Memo,
            createdRecord.CreatedAt,
            createdRecord.UpdatedAt,
            status
        );
    }
}
