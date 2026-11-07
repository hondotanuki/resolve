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
        Assert.Equal(DateTimeKind.Utc, record.StudiedAt.Kind);
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

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void StudiedAtがUTC以外なら生成できない(DateTimeKind kind)
    {
        var studiedAt = new DateTime(2026, 10, 15, 9, 0, 0, kind);

        var exception = Assert.Throws<ArgumentException>(() =>
            new StudyRecord(1, StudyResult.Success, studiedAt)
        );

        Assert.Equal("studiedAt", exception.ParamName);
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

    [Fact]
    public void 有効な入力でStudyRecordを訂正できる()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前"
        );

        var originalId = record.Id;
        var originalLearningItemId = record.LearningItemId;
        var originalCreatedAt = record.CreatedAt;

        var updatedStudiedAt = StudiedAt.AddDays(1);
        var updatedResult = StudyResult.Failure;

        var before = DateTime.UtcNow;

        record.Update(updatedResult, updatedStudiedAt, durationSeconds: 600, memo: "訂正後");

        var after = DateTime.UtcNow;

        Assert.Equal(updatedStudiedAt, record.StudiedAt);
        Assert.Equal(updatedResult, record.Result);
        Assert.Equal(600, record.DurationSeconds);
        Assert.Equal("訂正後", record.Memo);

        Assert.Equal(originalId, record.Id);
        Assert.Equal(originalLearningItemId, record.LearningItemId);
        Assert.Equal(originalCreatedAt, record.CreatedAt);

        Assert.InRange(record.UpdatedAt, before, after);
    }

    [Fact]
    public void DurationSecondsとMemoをnullに訂正できる()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前"
        );

        record.Update(StudyResult.Success, StudiedAt, durationSeconds: null, memo: null);

        Assert.Null(record.DurationSeconds);
        Assert.Null(record.Memo);
    }

    [Fact]
    public void StudiedAtが未設定なら訂正できない()
    {
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt);

        Assert.Throws<ArgumentException>(() =>
            record.Update(StudyResult.Success, default, durationSeconds: null, memo: null)
        );
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void StudiedAtがUTC以外なら訂正できない(DateTimeKind kind)
    {
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt);
        var studiedAt = new DateTime(2026, 10, 16, 9, 0, 0, kind);

        var exception = Assert.Throws<ArgumentException>(() =>
            record.Update(StudyResult.Success, studiedAt, null, null)
        );

        Assert.Equal("studiedAt", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void StudyResultが不正なら訂正できない(int value)
    {
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            record.Update((StudyResult)value, StudiedAt, null, null)
        );
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-60)]
    public void DurationSecondsが負なら訂正できない(int durationSeconds)
    {
        var record = new StudyRecord(1, StudyResult.Success, StudiedAt);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            record.Update(StudyResult.Success, StudiedAt, durationSeconds, null)
        );
    }

    [Fact]
    public void 訂正に失敗した場合は元の値が維持される()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前"
        );

        var originalUpdatedAt = record.UpdatedAt;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            record.Update(
                StudyResult.Success,
                StudiedAt.AddDays(1),
                durationSeconds: -1,
                memo: "訂正後"
            )
        );

        Assert.Equal(StudiedAt, record.StudiedAt);
        Assert.Equal(StudyResult.Success, record.Result);
        Assert.Equal(1800, record.DurationSeconds);
        Assert.Equal("訂正前", record.Memo);
        Assert.Equal(originalUpdatedAt, record.UpdatedAt);
    }
}
