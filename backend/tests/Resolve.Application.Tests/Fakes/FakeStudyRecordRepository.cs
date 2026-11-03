using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Application.Tests.Fakes;

public sealed class FakeStudyRecordRepository : IStudyRecordRepository
{
    public int CreateCallCount { get; private set; }
    public int GetLatestCallCount { get; private set; }
    public StudyRecord? ReceivedRecord { get; private set; }
    public StudyRecord? CreateResult { get; set; }
    public StudyRecord? LatestRecord { get; set; }
    public int? LastRequestedLearningItemId { get; private set; }

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

    public Task<StudyRecord?> GetLatestByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    )
    {
        GetLatestCallCount++;
        LastRequestedLearningItemId = learningItemId;

        return Task.FromResult(LatestRecord);
    }
}
