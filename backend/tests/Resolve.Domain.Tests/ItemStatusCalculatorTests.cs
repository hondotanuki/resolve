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
    [InlineData(StudyResult.Reviewed, ItemStatus.Completed)]
    public void 最新の学習結果から状態を算出できる(StudyResult result, ItemStatus expected)
    {
        var records = new[] { new StudyRecord(1, result, StudiedAt) };
        var status = ItemStatusCalculator.Calculate(records);

        Assert.Equal(expected, status);
    }

    [Fact]
    public void 記録の並び順ではなくStudiedAtが最新の記録を使う()
    {
        var records = new[]
        {
            new StudyRecord(1, StudyResult.Failure, StudiedAt),
            new StudyRecord(1, StudyResult.Success, StudiedAt.AddDays(-2)),
        };
        var status = ItemStatusCalculator.Calculate(records);

        Assert.Equal(ItemStatus.NeedsReview, status);
    }
}
