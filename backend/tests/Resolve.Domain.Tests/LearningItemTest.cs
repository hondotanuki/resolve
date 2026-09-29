namespace Resolve.Domain.Tests;

using Resolve.Domain;

public class LearningItemTests
{
    [Fact]
    public void 有効な値でLearningItemを生成できる()
    {
        var item = new LearningItem(ItemType.AlgorithmProblem, "タイトル", "本文");

        Assert.Equal(ItemType.AlgorithmProblem, item.ItemType);
        Assert.Equal("タイトル", item.Title);
        Assert.Equal("本文", item.Content);
    }

    [Fact]
    public void ContentなしでLearningItemを生成できる()
    {
        var item = new LearningItem(ItemType.EnglishWord, "単語", null);

        Assert.Null(item.Content);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("　")]
    public void タイトルが空白の場合は生成できない(string title)
    {
        Assert.Throws<ArgumentException>(() => new LearningItem(ItemType.EnglishWord, title, null));
    }

    [Fact]
    public void 未定義のItemTypeは拒否する()
    {
        var invalidType = (ItemType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LearningItem(invalidType, "タイトル", null)
        );
    }
}
