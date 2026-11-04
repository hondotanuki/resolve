using Resolve.Domain;

namespace Resolve.Application.StudyRecords.List;

public sealed record ListStudyRecordsOutput(
    int Id,
    int LearningItemId,
    StudyResult Result,
    DateTime StudiedAt,
    int? DurationSeconds,
    string? Memo
);
