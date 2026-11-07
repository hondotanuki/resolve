using Resolve.Api.Contracts;

namespace Resolve.Api.StudyRecords.Create;

public sealed record CreateStudyRecordRequest
{
    [ApiAllowedValues(ApiValueSet.StudyResults)]
    public required string Result { get; init; }
    public required DateTime StudiedAt { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Memo { get; init; }
}
