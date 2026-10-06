using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class ListLearningItemsUseCaseTests
{
    private readonly FakeLearningItemRepository _repository;
    private readonly ListLearningItemsUseCase _useCase;

    public ListLearningItemsUseCaseTests()
    {
        _repository = new FakeLearningItemRepository();
        _useCase = new ListLearningItemsUseCase(_repository);
    }

    [Fact]
    public async Task 登録済み項目をOutputへ変換できる()
    {
        var item = new LearningItem(ItemType.AlgorithmProblem, "二分探索", "二分探索を解く");

        _repository.ListResult = [item];

        var output = await _useCase.ExecuteAsync();

        Assert.Single(output);

        var actual = output[0];

        Assert.Equal(item.Id, actual.Id);
        Assert.Equal(item.ItemType, actual.ItemType);
        Assert.Equal(item.Title, actual.Title);
        Assert.Equal(item.ArchivedAt, actual.ArchivedAt);
    }

    [Fact]
    public async Task データがなければ空配列を返す()
    {
        _repository.ListResult = [];

        var output = await _useCase.ExecuteAsync();

        Assert.Empty(output);
    }
}
