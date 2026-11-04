using System.Text.Json.Serialization;

namespace Resolve.Api.StudyRecords.Create;

public sealed record CreateStudyRecordRequest
{
    public required string Result { get; init; }

    [JsonConverter(typeof(UtcDateTimeJsonConverter))]
    public required DateTime StudiedAt { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Memo { get; init; }
}
