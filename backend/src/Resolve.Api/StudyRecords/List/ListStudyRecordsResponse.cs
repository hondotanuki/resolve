using Resolve.Api.Contracts;

namespace Resolve.Api.StudyRecords.List;

public sealed record ListStudyRecordsResponse
{
    public required int Id { get; init; }
    public required int LearningItemId { get; init; }

    [ApiAllowedValues(ApiValueSet.StudyResults)]
    public required string Result { get; init; }
    public required DateTime StudiedAt { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Memo { get; init; }
}
