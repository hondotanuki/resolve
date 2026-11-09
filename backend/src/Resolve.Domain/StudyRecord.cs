namespace Resolve.Domain;

public sealed class StudyRecord
{
    public int Id { get; private set; }
    public int LearningItemId { get; private set; }
    public StudyResult Result { get; private set; }
    public DateTime StudiedAt { get; private set; }
    public int DurationSeconds { get; private set; }
    public int TargetSeconds { get; private set; }
    public bool IsOvertime => StudyTime.IsOvertime(DurationSeconds, TargetSeconds);
    public string? Memo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public StudyRecord(
        int learningItemId,
        StudyResult result,
        DateTime studiedAt,
        int durationSeconds,
        int targetSeconds,
        string? memo = null
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(learningItemId);
        Validate(result, studiedAt, durationSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetSeconds);

        LearningItemId = learningItemId;
        Result = result;
        StudiedAt = studiedAt;
        DurationSeconds = durationSeconds;
        TargetSeconds = targetSeconds;
        Memo = memo;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Update(StudyResult result, DateTime studiedAt, int durationSeconds, string? memo)
    {
        Validate(result, studiedAt, durationSeconds);

        Result = result;
        StudiedAt = studiedAt;
        DurationSeconds = durationSeconds;
        Memo = memo;

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(StudyResult result, DateTime studiedAt, int durationSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds);

        if (!Enum.IsDefined(result))
        {
            throw new ArgumentOutOfRangeException(
                nameof(result),
                result,
                "対応していない学習結果です。"
            );
        }

        if (studiedAt == default)
        {
            throw new ArgumentException("学習日時を指定してください。", nameof(studiedAt));
        }

        if (studiedAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("学習日時はUTCで指定してください。", nameof(studiedAt));
        }
    }
}
