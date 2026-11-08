using Resolve.Application.LearningItems.Create;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

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
            Source = ProblemSource.AtCoder,
            Category = ContestCategory.Beginner,
            ProblemIndex = 'A',
            Title = "二分探索",
        };
        var output = await _useCase.ExecuteAsync(input);

        Assert.Equal("二分探索", output.Title);
        Assert.Equal(ProblemSource.AtCoder, output.Source);
        Assert.Equal(ContestCategory.Beginner, output.Category);
        Assert.Equal('A', output.ProblemIndex);
    }

    [Fact]
    public async Task 有効な入力でRepositoryが1回呼ばれる()
    {
        var input = new CreateLearningItemInput
        {
            Source = ProblemSource.AtCoder,
            Category = ContestCategory.Beginner,
            ProblemIndex = 'A',
            Title = "二分探索",
        };
        await _useCase.ExecuteAsync(input);

        Assert.Equal(1, _repository.CreateCallCount);
    }

    [Fact]
    public async Task Repositoryに正しいLearningItemが渡される()
    {
        var input = new CreateLearningItemInput
        {
            Source = ProblemSource.AtCoder,
            Category = ContestCategory.Beginner,
            ProblemIndex = 'A',
            Title = "二分探索",
        };
        await _useCase.ExecuteAsync(input);

        var item = _repository.ReceivedItem;

        Assert.NotNull(item);

        Assert.Equal("二分探索", item.Title);
        Assert.Equal(ProblemSource.AtCoder, item.Source);
        Assert.Equal(ContestCategory.Beginner, item.Category);
        Assert.Equal('A', item.ProblemIndex);
    }

    [Fact]
    public async Task 登録時に問題区分を返す()
    {
        var input = new CreateLearningItemInput
        {
            Source = ProblemSource.AtCoder,
            Category = ContestCategory.Beginner,
            ProblemIndex = 'A',
            Title = "二分探索",
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.Equal(1, _repository.CreateCallCount);
        Assert.Equal(ProblemSource.AtCoder, output.Source);
        Assert.Equal(ContestCategory.Beginner, output.Category);
        Assert.Equal('A', output.ProblemIndex);
    }

    [Fact]
    public async Task 空タイトルではRepositoryが呼ばれない()
    {
        var input = new CreateLearningItemInput
        {
            Source = ProblemSource.AtCoder,
            Category = ContestCategory.Beginner,
            ProblemIndex = 'A',
            Title = "",
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));
        Assert.Equal(0, _repository.CreateCallCount);
    }

    [Fact]
    public async Task OutputにRepositoryの保存結果が反映される()
    {
        var savedItem = new LearningItem(
            "保存後タイトル",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

        _repository.CreateResult = savedItem;

        var input = new CreateLearningItemInput
        {
            Source = ProblemSource.AtCoder,
            Category = ContestCategory.Beginner,
            ProblemIndex = 'A',
            Title = "入力タイトル",
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.Equal(savedItem.Id, output.Id);

        Assert.Equal(savedItem.Title, output.Title);

        Assert.Equal(savedItem.ArchivedAt, output.ArchivedAt);
        Assert.Equal(savedItem.CreatedAt, output.CreatedAt);
        Assert.Equal(savedItem.UpdatedAt, output.UpdatedAt);
    }
}
