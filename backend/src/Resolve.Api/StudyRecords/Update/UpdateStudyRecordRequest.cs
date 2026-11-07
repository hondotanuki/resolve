using System.Text.Json;
using System.Text.Json.Serialization;
using Resolve.Api.Contracts;

namespace Resolve.Api.StudyRecords.Update;

public sealed record UpdateStudyRecordRequest
{
    private string _result = string.Empty;
    private DateTime _studiedAt;
    private int? _durationSeconds;
    private string? _memo;

    [JsonIgnore]
    public bool ResultIsSpecified { get; private set; }

    [JsonIgnore]
    public bool StudiedAtIsSpecified { get; private set; }

    [JsonIgnore]
    public bool DurationSecondsIsSpecified { get; private set; }

    [JsonIgnore]
    public bool MemoIsSpecified { get; private set; }

    [ApiAllowedValues(ApiValueSet.StudyResults)]
    public string Result
    {
        get => _result;
        init
        {
            _result = value ?? throw new JsonException("Result cannot be null.");
            ResultIsSpecified = true;
        }
    }

    public DateTime StudiedAt
    {
        get => _studiedAt;
        init
        {
            _studiedAt = value;
            StudiedAtIsSpecified = true;
        }
    }

    public int? DurationSeconds
    {
        get => _durationSeconds;
        init
        {
            _durationSeconds = value;
            DurationSecondsIsSpecified = true;
        }
    }
    public string? Memo
    {
        get => _memo;
        init
        {
            _memo = value;
            MemoIsSpecified = true;
        }
    }
}
