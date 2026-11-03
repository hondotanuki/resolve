namespace Resolve.Domain.Tests;

public sealed class StudyRecordTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void 有効な入力でStudyRecordを生成できる()
    {
        var before = DateTime.UtcNow;
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt);
        var after = DateTime.UtcNow;

        Assert.Equal(1, record.LearningItemId);
        Assert.Equal(StudyResult.Success, record.Result);
        Assert.Equal(StudiedAt, record.StudiedAt);
        Assert.InRange(record.CreatedAt, before, after);
        Assert.Equal(record.CreatedAt, record.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LearningItemIdが不正なら生成できない(int id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StudyRecord(id, StudyResult.Success, StudiedAt)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void StudyResultが不正なら生成できない(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StudyRecord(1, (StudyResult)value, StudiedAt)
        );
    }

    [Fact]
    public void StudiedAtが未設定なら生成できない()
    {
        Assert.Throws<ArgumentException>(() => new StudyRecord(1, StudyResult.Success, default));
    }

    [Fact]
    public void DurationSecondsとMemoを省略できる()
    {
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt);

        Assert.Null(record.DurationSeconds);
        Assert.Null(record.Memo);
    }

    [Fact]
    public void DurationSecondsとMemoを指定できる()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "二分探索を復習した"
        );

        Assert.Equal(1800, record.DurationSeconds);
        Assert.Equal("二分探索を復習した", record.Memo);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-60)]
    public void DurationSecondsが負なら生成できない(int durationSeconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StudyRecord(1, StudyResult.Success, StudiedAt, durationSeconds: durationSeconds)
        );
    }

    [Fact]
    public void DurationSecondsが0でも生成できる()
    {
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt, durationSeconds: 0);

        Assert.Equal(0, record.DurationSeconds);
    }
}
