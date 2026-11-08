namespace Resolve.Domain;

public static class StudyTime
{
    public static bool IsOvertime(int durationSeconds, int targetSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(durationSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetSeconds);

        return durationSeconds > targetSeconds;
    }
}
