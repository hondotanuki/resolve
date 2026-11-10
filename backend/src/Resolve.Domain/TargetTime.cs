namespace Resolve.Domain;

public static class TargetTime
{
    private static readonly int[] AtCoderBeginnerMinutes = [10, 10, 20, 20, 20, 20, 40];
    private static readonly int[] AtCoderRegularMinutes = [30, 40, 50, 100, 100, 100, 100];
    private static readonly int[] OMCBeginnerMinutes = [5, 10, 10, 10, 15, 15, 20];
    private static readonly int[] OMCRegularMinutes = [10, 10, 20, 20, 20, 20, 40];
    private static readonly int[] OMCExpertMinutes = [30, 40, 50, 100, 100, 100, 100];

    public static int? GetSeconds(ProblemSource source, ContestCategory category, char problemIndex)
    {
        // AGC has a fixed target time for all problem letters.
        if (problemIndex is < 'A' or > 'Z')
        {
            return null;
        }

        if (source == ProblemSource.AtCoder && category == ContestCategory.Grand)
        {
            return 120 * 60;
        }

        int[]? minutes = (source, category) switch
        {
            (ProblemSource.AtCoder, ContestCategory.Beginner) => AtCoderBeginnerMinutes,
            (ProblemSource.AtCoder, ContestCategory.Regular or ContestCategory.RegularPlus) =>
                AtCoderRegularMinutes,
            (ProblemSource.OnlineMathContest, ContestCategory.Beginner) => OMCBeginnerMinutes,
            (ProblemSource.OnlineMathContest, ContestCategory.Regular) => OMCRegularMinutes,
            (ProblemSource.OnlineMathContest, ContestCategory.Expert) => OMCExpertMinutes,
            _ => null,
        };

        int index = problemIndex - 'A';
        return minutes is not null && index < minutes.Length ? minutes[index] * 60 : null;
    }
}
