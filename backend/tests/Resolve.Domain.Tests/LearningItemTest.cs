namespace Resolve.Domain.Tests;

public class LearningItemTests
{
    [Fact]
    public void 有効な値でLearningItemを生成できる()
    {
        var item = new LearningItem(
            ItemType.AlgorithmProblem,
            "タイトル",
            "本文",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

        Assert.Equal(ItemType.AlgorithmProblem, item.ItemType);
        Assert.Equal("タイトル", item.Title);
        Assert.Equal("本文", item.Content);
        Assert.Equal(ProblemSource.AtCoder, item.Source);
        Assert.Equal(ContestCategory.Beginner, item.Category);
        Assert.Equal('A', item.ProblemIndex);
    }

    [Fact]
    public void ContentなしでLearningItemを生成できる()
    {
        var item = new LearningItem(
            ItemType.EnglishWord,
            "単語",
            null,
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

        Assert.Null(item.Content);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("　")]
    public void タイトルが空白の場合は生成できない(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new LearningItem(
                ItemType.EnglishWord,
                title,
                null,
                source: ProblemSource.AtCoder,
                category: ContestCategory.Beginner,
                problemIndex: 'A'
            )
        );
    }

    [Fact]
    public void 未定義のItemTypeは拒否する()
    {
        var invalidType = (ItemType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LearningItem(
                invalidType,
                "タイトル",
                null,
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
            new LearningItem(
                ItemType.AlgorithmProblem,
                "問題",
                null,
                source,
                category,
                problemIndex
            )
        );
    }
}
