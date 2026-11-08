namespace Resolve.Domain.Tests;

public sealed class ItemStatusCalculatorTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void 学習記録がなければUnstartedになる()
    {
        var records = Array.Empty<StudyRecord>();
        var status = ItemStatusCalculator.Calculate(records);

        Assert.Equal(ItemStatus.Unstarted, status);
    }

    [Theory]
    [InlineData(StudyResult.Success, ItemStatus.Completed)]
    [InlineData(StudyResult.Failure, ItemStatus.NeedsReview)]
    public void 最新の学習結果から状態を算出できる(StudyResult result, ItemStatus expected)
    {
        var records = new[]
        {
            new StudyRecord(1, result, StudiedAt, durationSeconds: 0, targetSeconds: 600),
        };
        var status = ItemStatusCalculator.Calculate(records);

        Assert.Equal(expected, status);
    }

    [Fact]
    public void 記録の並び順ではなくStudiedAtが最新の記録を使う()
    {
        var records = new[]
        {
            new StudyRecord(
                1,
                StudyResult.Failure,
                StudiedAt,
                durationSeconds: 0,
                targetSeconds: 600
            ),
            new StudyRecord(
                1,
                StudyResult.Success,
                StudiedAt.AddDays(-2),
                durationSeconds: 0,
                targetSeconds: 600
            ),
        };
        var status = ItemStatusCalculator.Calculate(records);

        Assert.Equal(ItemStatus.NeedsReview, status);
    }

    [Theory]
    [InlineData(StudyResult.Success, 600, ItemStatus.Completed)]
    [InlineData(StudyResult.Success, 601, ItemStatus.NeedsReview)]
    [InlineData(StudyResult.Failure, 0, ItemStatus.NeedsReview)]
    public void 正誤と保存時の目標時間から状態を算出する(
        StudyResult result,
        int duration,
        ItemStatus expected
    )
    {
        var record = new StudyRecord(1, result, StudiedAt, duration, 600);
        Assert.Equal(expected, ItemStatusCalculator.Calculate([record]));
    }
}
