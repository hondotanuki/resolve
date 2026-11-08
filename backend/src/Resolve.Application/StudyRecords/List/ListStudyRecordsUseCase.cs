using Resolve.Application.Interfaces;

namespace Resolve.Application.StudyRecords.List;

public sealed class ListStudyRecordsUseCase
{
    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IStudyRecordRepository _studyRecordRepository;

    public ListStudyRecordsUseCase(
        ILearningItemRepository learningItemRepository,
        IStudyRecordRepository studyRecordRepository
    )
    {
        _learningItemRepository = learningItemRepository;
        _studyRecordRepository = studyRecordRepository;
    }

    public async Task<IReadOnlyList<ListStudyRecordsOutput>?> ExecuteAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    )
    {
        var learningItem = await _learningItemRepository.GetByIdAsync(
            learningItemId,
            cancellationToken
        );
        if (learningItem is null)
        {
            return null;
        }

        var items = await _studyRecordRepository.ListByLearningItemIdAsync(
            learningItemId,
            cancellationToken
        );

        return items
            .Select(x => new ListStudyRecordsOutput(
                x.Id,
                x.LearningItemId,
                x.Result,
                x.StudiedAt,
                x.DurationSeconds,
                x.TargetSeconds,
                x.Memo
            ))
            .ToList();
    }
}
