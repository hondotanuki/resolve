using Resolve.Domain;

namespace Resolve.Application.StudyRecords.Create;

public sealed record CreateStudyRecordInput
{
    public required int LearningItemId { get; init; }
    public required StudyResult Result { get; init; }
    public required DateTime StudiedAt { get; init; }
    public required int DurationSeconds { get; init; }
    public string? Memo { get; init; }
}
