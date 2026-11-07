using Resolve.Api.Contracts;
using Resolve.Domain;

namespace Resolve.Api.StudyRecords.Update;

public sealed record UpdateStudyRecordResponse
{
    public required int Id { get; init; }
    public required int LearningItemId { get; init; }

    [ApiAllowedValues(ApiValueSet.StudyResults)]
    public required string Result { get; init; }
    public required DateTime StudiedAt { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Memo { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
