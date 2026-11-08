using Resolve.Api.Contracts;

namespace Resolve.Api.StudyRecords.Create;

public sealed record CreateStudyRecordResponse
{
    public required int Id { get; init; }
    public required int LearningItemId { get; init; }

    [ApiAllowedValues(ApiValueSet.StudyResults)]
    public required string Result { get; init; }
    public required DateTime StudiedAt { get; init; }
    public required int DurationSeconds { get; init; }
    public required int TargetSeconds { get; init; }
    public required string? Memo { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    [ApiAllowedValues(ApiValueSet.ItemStatuses)]
    public required string Status { get; init; }
}
