using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Update;

public sealed record UpdateStudyRecordOutput(
    int Id,
    int LearningItemId,
    DateTime StudiedAt,
    StudyResult Result,
    int? DurationSeconds,
    string? Memo,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
