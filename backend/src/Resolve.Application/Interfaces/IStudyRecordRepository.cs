using Resolve.Domain;

namespace Resolve.Application.Interfaces;

public interface IStudyRecordRepository
{
    Task<StudyRecord> CreateAsync(
        StudyRecord studyRecord,
        CancellationToken cancellationToken = default
    );
    Task<StudyRecord?> GetLatestByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    );
}
