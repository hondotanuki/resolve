namespace Resolve.Domain.Tests;

public class LearningItemTests
{
    [Fact]
    public void 有効な値でLearningItemを生成できる()
    {
        var item = new LearningItem(
            "タイトル",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A',
            problemUrl: "https://atcoder.jp/contests/abc482/tasks/abc482_a",
            contestId: "abc482",
            contestName: "ABC 482",
            contestStartDate: new DateOnly(2026, 10, 31)
        );

        Assert.Equal("タイトル", item.Title);

        Assert.Equal(ProblemSource.AtCoder, item.Source);
        Assert.Equal(ContestCategory.Beginner, item.Category);
        Assert.Equal('A', item.ProblemIndex);
        Assert.Equal("abc482", item.ContestId);
        Assert.Equal("ABC 482", item.ContestName);
        Assert.Equal(new DateOnly(2026, 10, 31), item.ContestStartDate);
    }

    [Fact]
    public void 問題URLとコンテスト情報を指定して生成できる()
    {
        var date = new DateOnly(2026, 10, 31);
        var item = new LearningItem(
            "Nigiri",
            ProblemSource.AtCoder,
            ContestCategory.Beginner,
            'A',
            "https://atcoder.jp/contests/abc482/tasks/abc482_a",
            "abc482",
            "AtCoder Beginner Contest 482",
            date
        );

        Assert.Equal("https://atcoder.jp/contests/abc482/tasks/abc482_a", item.ProblemUrl);
        Assert.Equal("abc482", item.ContestId);
        Assert.Equal("AtCoder Beginner Contest 482", item.ContestName);
        Assert.Equal(date, item.ContestStartDate);
    }

    [Theory]
    [InlineData("", "abc482", "ABC 482")]
    [InlineData("https://atcoder.jp/contests/abc482/tasks/abc482_a", " ", "ABC 482")]
    [InlineData("https://atcoder.jp/contests/abc482/tasks/abc482_a", "abc482", "")]
    public void URLとコンテスト情報が空なら生成できない(
        string url,
        string contestId,
        string contestName
    )
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new LearningItem(
                "問題",
                ProblemSource.AtCoder,
                ContestCategory.Beginner,
                'A',
                url,
                contestId,
                contestName,
                new DateOnly(2026, 10, 31)
            )
        );
    }

    [Fact]
    public void 開始日が初期値なら生成できない()
    {
        Assert.Throws<ArgumentException>(() =>
            new LearningItem(
                "問題",
                ProblemSource.AtCoder,
                ContestCategory.Beginner,
                'A',
                "https://atcoder.jp/contests/abc482/tasks/abc482_a",
                "abc482",
                "ABC 482",
                default
            )
        );
    }

    [Fact]
    public void 不正な区分への変更では元の区分を保持する()
    {
        var item = new LearningItem(
            "問題",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A',
            problemUrl: "https://atcoder.jp/contests/abc482/tasks/abc482_a",
            contestId: "abc482",
            contestName: "ABC 482",
            contestStartDate: new DateOnly(2026, 10, 31)
        );
        Assert.Throws<ArgumentException>(() =>
            item.SetProblemCategory(ProblemSource.AtCoder, ContestCategory.Beginner, 'H')
        );
        Assert.Equal('A', item.ProblemIndex);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("　")]
    public void タイトルが空白の場合は生成できない(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new LearningItem(
                title,
                source: ProblemSource.AtCoder,
                category: ContestCategory.Beginner,
                problemIndex: 'A',
                problemUrl: "https://atcoder.jp/contests/abc482/tasks/abc482_a",
                contestId: "abc482",
                contestName: "ABC 482",
                contestStartDate: new DateOnly(2026, 10, 31)
            )
        );
    }

    [Theory]
    [InlineData(ProblemSource.AtCoder, ContestCategory.Beginner, 'H')]
    [InlineData(ProblemSource.OnlineMathContest, ContestCategory.Grand, 'A')]
    [InlineData((ProblemSource)999, ContestCategory.Beginner, 'A')]
    public void 未対応の問題区分では生成できない(
        ProblemSource source,
        ContestCategory category,
        char problemIndex
    )
    {
        Assert.Throws<ArgumentException>(() =>
            new LearningItem(
                "問題",
                source,
                category,
                problemIndex,
                problemUrl: "https://atcoder.jp/contests/abc482/tasks/abc482_a",
                contestId: "abc482",
                contestName: "ABC 482",
                contestStartDate: new DateOnly(2026, 10, 31)
            )
        );
    }
}
