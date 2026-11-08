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
            problemIndex: 'A'
        );

        Assert.Equal("タイトル", item.Title);

        Assert.Equal(ProblemSource.AtCoder, item.Source);
        Assert.Equal(ContestCategory.Beginner, item.Category);
        Assert.Equal('A', item.ProblemIndex);
    }

    [Fact]
    public void 不正な区分への変更では元の区分を保持する()
    {
        var item = new LearningItem(
            "問題",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
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
                problemIndex: 'A'
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
            new LearningItem("問題", source, category, problemIndex)
        );
    }
}
