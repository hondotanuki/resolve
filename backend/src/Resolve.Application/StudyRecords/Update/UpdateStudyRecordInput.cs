using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Update;

public sealed record UpdateStudyRecordInput
{
    public required int Id { get; init; }
    public OptionalValue<StudyResult> Result { get; init; }
    public OptionalValue<DateTime> StudiedAt { get; init; }
    public OptionalValue<int> DurationSeconds { get; init; }
    public OptionalValue<string?> Memo { get; init; }
}
