using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Create;

public sealed record CreateStudyRecordOutput(
    int Id,
    int LearningItemId,
    StudyResult Result,
    DateTime StudiedAt,
    int DurationSeconds,
    int TargetSeconds,
    string? Memo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ItemStatus Status
);
