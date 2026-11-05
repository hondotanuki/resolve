using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.Tests.Fakes;

public sealed class FakeStudyRecordRepository : IStudyRecordRepository
{
    public int CreateCallCount { get; private set; }
    public int GetLatestCallCount { get; private set; }
    public int ListCallCount { get; private set; }
    public int GetByIdCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public int DeleteCallCount { get; private set; }

    public StudyRecord? ReceivedRecord { get; private set; }
    public StudyRecord? ReceivedUpdatedRecord { get; private set; }
    public StudyRecord? ReceivedDeletedRecord { get; private set; }
    public StudyRecord? CreateResult { get; set; }
    public StudyRecord? LatestRecord { get; set; }
    public StudyRecord? RecordById { get; set; }
    public IReadOnlyList<StudyRecord> ListResult { get; set; } = [];

    public int? LastRequestedLearningItemId { get; private set; }
    public int? LastRequestedStudyRecordId { get; private set; }

    public Task<StudyRecord> CreateAsync(
        StudyRecord studyRecord,
        CancellationToken cancellationToken = default
    )
    {
        CreateCallCount++;
        ReceivedRecord = studyRecord;

        return Task.FromResult(
            CreateResult ?? throw new InvalidOperationException("CreateResult must be configured.")
        );
    }

    public Task<StudyRecord?> GetByIdAsync(
        int studyRecordId,
        CancellationToken cancellationToken = default
    )
    {
        GetByIdCallCount++;
        LastRequestedStudyRecordId = studyRecordId;

        return Task.FromResult(RecordById);
    }

    public Task UpdateAsync(StudyRecord studyRecord, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        ReceivedUpdatedRecord = studyRecord;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(StudyRecord studyRecord, CancellationToken cancellationToken = default)
    {
        DeleteCallCount++;
        ReceivedDeletedRecord = studyRecord;

        return Task.CompletedTask;
    }

    public Task<StudyRecord?> GetLatestByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    )
    {
        GetLatestCallCount++;
        LastRequestedLearningItemId = learningItemId;

        return Task.FromResult(LatestRecord);
    }

    public Task<IReadOnlyList<StudyRecord>> ListByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    )
    {
        ListCallCount++;
        LastRequestedLearningItemId = learningItemId;

        return Task.FromResult(ListResult);
    }
}
