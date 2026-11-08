namespace Resolve.Domain.Tests;

public sealed class StudyTimeTests
{
    [Theory]
    [InlineData(0, 600, false)]
    [InlineData(599, 600, false)]
    [InlineData(600, 600, false)]
    [InlineData(601, 600, true)]
    public void 目標時間を超えた場合のみ時間超過になる(
        int durationSeconds,
        int targetSeconds,
        bool expected
    )
    {
        Assert.Equal(expected, StudyTime.IsOvertime(durationSeconds, targetSeconds));
    }

    [Theory]
    [InlineData(-1, 600)]
    [InlineData(0, 0)]
    [InlineData(0, -1)]
    public void 不正な時間は拒否する(int durationSeconds, int targetSeconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StudyTime.IsOvertime(durationSeconds, targetSeconds)
        );
    }
}
