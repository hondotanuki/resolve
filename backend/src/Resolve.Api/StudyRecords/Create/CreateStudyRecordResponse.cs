namespace Resolve.Api.StudyRecords.Create;

public sealed record CreateStudyRecordResponse
{
    public required int Id { get; init; }
    public required int LearningItemId { get; init; }
    public required string Result { get; init; }
    public required DateTime StudiedAt { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Memo { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string Status { get; init; }
}
