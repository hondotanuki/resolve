using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Update;

public sealed record UpdateStudyRecordInput(
    int StudyRecordId,
    OptionalValue<DateTime> StudiedAt,
    OptionalValue<StudyResult> Result,
    OptionalValue<int?> DurationSeconds,
    OptionalValue<string?> Memo
);
