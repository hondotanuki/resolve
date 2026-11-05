using Resolve.Domain;

namespace Resolve.Application.Interfaces;

public interface IStudyRecordRepository
{
    Task<StudyRecord> CreateAsync(
        StudyRecord studyRecord,
        CancellationToken cancellationToken = default
    );
    Task<StudyRecord?> GetByIdAsync(
        int studyRecordId,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(StudyRecord studyRecord, CancellationToken cancellationToken = default);
    Task DeleteAsync(StudyRecord studyRecord, CancellationToken cancellationToken = default);
    Task<StudyRecord?> GetLatestByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<StudyRecord>> ListByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    );
}
