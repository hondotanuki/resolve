using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Resolve.Api.Contracts;

namespace Resolve.Api.StudyRecords.Create;

public sealed record CreateStudyRecordRequest
{
    [ApiAllowedValues(ApiValueSet.StudyResults)]
    public required string Result { get; init; }

    [Description("UTCの日時をZ付きで指定してください。DateTimeの初期値は指定できません。")]
    public required DateTime StudiedAt { get; init; }

    [Range(0, int.MaxValue)]
    public required int DurationSeconds { get; init; }
    public string? Memo { get; init; }
}
