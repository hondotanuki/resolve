namespace Resolve.Application.Tests;

using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

public class CreateLearningItemUseCaseTests
{
    private readonly FakeLearningItemRepository _repository;
    private readonly CreateLearningItemUseCase _useCase;

    public CreateLearningItemUseCaseTests()
    {
        _repository = new FakeLearningItemRepository();
        _useCase = new CreateLearningItemUseCase(_repository);
    }

    [Fact]
    public async Task 有効な入力でUseCaseが実行できる()
    {
        var input = new CreateLearningItemInput
        {
            ItemType = ItemType.AlgorithmProblem,
            Title = "二分探索",
            Content = "二分探索の問題を解く",
        };
        var output = await _useCase.ExecuteAsync(input);

        Assert.Equal(ItemType.AlgorithmProblem, output.ItemType);
        Assert.Equal("二分探索", output.Title);
        Assert.Equal("二分探索の問題を解く", output.Content);
    }

    [Fact]
    public async Task 有効な入力でRepositoryが1回呼ばれる()
    {
        var input = new CreateLearningItemInput
        {
            ItemType = ItemType.AlgorithmProblem,
            Title = "二分探索",
            Content = "二分探索の問題を解く",
        };
        await _useCase.ExecuteAsync(input);

        Assert.Equal(1, _repository.CreateCallCount);
    }

    [Fact]
    public async Task Repositoryに正しいLearningItemが渡される()
    {
        var input = new CreateLearningItemInput
        {
            ItemType = ItemType.AlgorithmProblem,
            Title = "二分探索",
            Content = "二分探索の問題を解く",
        };
        await _useCase.ExecuteAsync(input);

        var item = _repository.ReceivedItem;

        Assert.NotNull(item);
        Assert.Equal(ItemType.AlgorithmProblem, item.ItemType);
        Assert.Equal("二分探索", item.Title);
        Assert.Equal("二分探索の問題を解く", item.Content);
    }

    [Fact]
    public async Task Contentなしでも登録できる()
    {
        var input = new CreateLearningItemInput
        {
            ItemType = ItemType.AlgorithmProblem,
            Title = "二分探索",
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.Equal(1, _repository.CreateCallCount);
        Assert.Null(_repository.ReceivedItem!.Content);
        Assert.Null(output.Content);
    }

    [Fact]
    public async Task 空タイトルではRepositoryが呼ばれない()
    {
        var input = new CreateLearningItemInput
        {
            ItemType = ItemType.AlgorithmProblem,
            Title = "",
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal(0, _repository.CreateCallCount);
    }

    [Fact]
    public async Task 不正なItemTypeではRepositoryが呼ばれない()
    {
        var input = new CreateLearningItemInput { ItemType = (ItemType)999, Title = "二分探索" };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal(0, _repository.CreateCallCount);
    }

    [Fact]
    public async Task OutputにRepositoryの保存結果が反映される()
    {
        var savedItem = new LearningItem(
            ItemType.AlgorithmProblem,
            "保存後タイトル",
            "保存後Content"
        );

        _repository.ItemToReturn = savedItem;

        var input = new CreateLearningItemInput
        {
            ItemType = ItemType.AlgorithmProblem,
            Title = "入力タイトル",
            Content = "入力Content",
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.Equal(savedItem.Id, output.Id);
        Assert.Equal(savedItem.ItemType, output.ItemType);
        Assert.Equal(savedItem.Title, output.Title);
        Assert.Equal(savedItem.Content, output.Content);
        Assert.Equal(savedItem.ArchivedAt, output.ArchivedAt);
        Assert.Equal(savedItem.CreatedAt, output.CreatedAt);
        Assert.Equal(savedItem.UpdatedAt, output.UpdatedAt);
    }
}
