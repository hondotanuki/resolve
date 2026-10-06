using Resolve.Application.LearningItems.GetById;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public class GetLearningItemByIdUseCaseTests
{
    private readonly FakeLearningItemRepository _repository;
    private readonly GetLearningItemByIdUseCase _useCase;

    public GetLearningItemByIdUseCaseTests()
    {
        _repository = new FakeLearningItemRepository();
        _useCase = new GetLearningItemByIdUseCase(_repository);
    }

    [Fact]
    public async Task 存在するIDなら詳細を返す()
    {
        var item = new LearningItem(ItemType.AlgorithmProblem, "二分探索", "二分探索を解く");

        _repository.GetByIdResult = item;
        var output = await _useCase.ExecuteAsync(1);

        Assert.NotNull(output);
        Assert.Equal(ItemType.AlgorithmProblem, output.ItemType);
        Assert.Equal("二分探索", output.Title);
        Assert.Equal("二分探索を解く", output.Content);
    }

    [Fact]
    public async Task 存在しないIDならnullを返す()
    {
        _repository.GetByIdResult = null;
        var output = await _useCase.ExecuteAsync(999);
        Assert.Null(output);
    }
}
