namespace Resolve.Domain.Tests;

public sealed class TargetTimeTests
{
    public static TheoryData<ProblemSource, ContestCategory, int[]> TargetMinutes =>
        new()
        {
            { ProblemSource.AtCoder, ContestCategory.Beginner, [10, 10, 20, 20, 20, 20, 40] },
            { ProblemSource.AtCoder, ContestCategory.Regular, [30, 40, 50, 100, 100, 100, 100] },
            {
                ProblemSource.AtCoder,
                ContestCategory.RegularPlus,
                [30, 40, 50, 100, 100, 100, 100]
            },
            {
                ProblemSource.OnlineMathContest,
                ContestCategory.Beginner,
                [5, 10, 10, 10, 15, 15, 20]
            },
            {
                ProblemSource.OnlineMathContest,
                ContestCategory.Regular,
                [10, 10, 20, 20, 20, 20, 40]
            },
            {
                ProblemSource.OnlineMathContest,
                ContestCategory.Expert,
                [30, 40, 50, 100, 100, 100, 100]
            },
        };

    [Theory]
    [MemberData(nameof(TargetMinutes))]
    public void 対応表の各問題区分に目標時間を返す(
        ProblemSource source,
        ContestCategory category,
        int[] expectedMinutes
    )
    {
        for (int index = 0; index < expectedMinutes.Length; index++)
        {
            Assert.Equal(
                expectedMinutes[index] * 60,
                TargetTime.GetSeconds(source, category, (char)('A' + index))
            );
        }
    }

    [Theory]
    [InlineData('A')]
    [InlineData('G')]
    [InlineData('H')]
    [InlineData('Z')]
    public void AGCの問題は120分を返す(char problemIndex)
    {
        Assert.Equal(
            120 * 60,
            TargetTime.GetSeconds(ProblemSource.AtCoder, ContestCategory.Grand, problemIndex)
        );
    }

    [Theory]
    [InlineData(ProblemSource.AtCoder, ContestCategory.Beginner, 'H')]
    [InlineData(ProblemSource.OnlineMathContest, ContestCategory.Expert, 'H')]
    [InlineData(ProblemSource.AtCoder, ContestCategory.Expert, 'A')]
    [InlineData(ProblemSource.OnlineMathContest, ContestCategory.Grand, 'A')]
    [InlineData(ProblemSource.OnlineMathContest, ContestCategory.RegularPlus, 'A')]
    [InlineData(ProblemSource.AtCoder, ContestCategory.Grand, 'a')]
    [InlineData(ProblemSource.AtCoder, ContestCategory.Grand, '1')]
    [InlineData((ProblemSource)999, ContestCategory.Beginner, 'A')]
    [InlineData(ProblemSource.AtCoder, (ContestCategory)999, 'A')]
    public void 未対応の区分には目標時間を返さない(
        ProblemSource source,
        ContestCategory category,
        char problemIndex
    )
    {
        Assert.Null(TargetTime.GetSeconds(source, category, problemIndex));
    }
}
