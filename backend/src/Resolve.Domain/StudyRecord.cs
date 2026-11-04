namespace Resolve.Domain;

public sealed class StudyRecord
{
    public int Id { get; private set; }
    public int LearningItemId { get; private set; }
    public StudyResult Result { get; private set; }
    public DateTime StudiedAt { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string? Memo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public StudyRecord(
        int learningItemId,
        StudyResult result,
        DateTime studiedAt,
        int? durationSeconds = null,
        string? memo = null
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(learningItemId);

        if (durationSeconds is < 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds.Value);
        }

        if (!Enum.IsDefined(result))
        {
            throw new ArgumentOutOfRangeException(nameof(result));
        }

        if (studiedAt == default)
        {
            throw new ArgumentException("StudiedAt must be specified.", nameof(studiedAt));
        }

        if (studiedAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("StudiedAt must be in UTC.", nameof(studiedAt));
        }

        LearningItemId = learningItemId;
        Result = result;
        StudiedAt = studiedAt;
        DurationSeconds = durationSeconds;
        Memo = memo;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
