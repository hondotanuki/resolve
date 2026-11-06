using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Update;

public sealed record UpdateStudyRecordOutput(
    int Id,
    int LearningItemId,
    StudyResult Result,
    DateTime StudiedAt,
    int? DurationSeconds,
    string? Memo,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
