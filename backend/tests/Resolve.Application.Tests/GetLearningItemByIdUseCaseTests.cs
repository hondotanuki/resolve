using Resolve.Application.LearningItems.GetById;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public class GetLearningItemByIdUseCaseTests
{
    private readonly FakeLearningItemRepository _learningItemRepository;
    private readonly FakeStudyRecordRepository _studyRecordRepository;
    private readonly GetLearningItemByIdUseCase _useCase;

    public GetLearningItemByIdUseCaseTests()
    {
        _learningItemRepository = new FakeLearningItemRepository();
        _studyRecordRepository = new FakeStudyRecordRepository();
        _useCase = new GetLearningItemByIdUseCase(_learningItemRepository, _studyRecordRepository);
    }

    [Fact]
    public async Task 存在するIDなら詳細を返す()
    {
        var item = new LearningItem(ItemType.AlgorithmProblem, "二分探索", "二分探索を解く");

        _learningItemRepository.GetByIdResult = item;
        var output = await _useCase.ExecuteAsync(1);

        Assert.NotNull(output);
        Assert.Equal(ItemType.AlgorithmProblem, output.ItemType);
        Assert.Equal("二分探索", output.Title);
        Assert.Equal("二分探索を解く", output.Content);
    }

    [Fact]
    public async Task 存在しないIDならnullを返す()
    {
        _learningItemRepository.GetByIdResult = null;
        var output = await _useCase.ExecuteAsync(999);
        Assert.Null(output);
        Assert.Equal(0, _studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task 記録なしならUnstartedを返す()
    {
        var item = new LearningItem(ItemType.AlgorithmProblem, "二分探索", "二分探索を解く");
        _learningItemRepository.GetByIdResult = item;
        _studyRecordRepository.LatestRecord = null;
        var output = await _useCase.ExecuteAsync(1);
        Assert.NotNull(output);
        Assert.Equal(ItemStatus.Unstarted, output.Status);
        Assert.Equal(1, _studyRecordRepository.GetLatestCallCount);
        Assert.Equal(1, _studyRecordRepository.LastRequestedLearningItemId);
    }

    [Theory]
    [InlineData(StudyResult.Failure, ItemStatus.NeedsReview)]
    [InlineData(StudyResult.Success, ItemStatus.Completed)]
    [InlineData(StudyResult.Reviewed, ItemStatus.Completed)]
    public async Task 最新の学習結果から現在状態を返す(StudyResult result, ItemStatus expected)
    {
        const int learningItemId = 42;
        _learningItemRepository.GetByIdResult = new LearningItem(ItemType.EnglishWord, "apple");
        _studyRecordRepository.LatestRecord = new StudyRecord(
            learningItemId,
            result,
            new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc)
        );

        var output = await _useCase.ExecuteAsync(learningItemId);

        Assert.NotNull(output);
        Assert.Equal(expected, output.Status);
        Assert.Equal(1, _studyRecordRepository.GetLatestCallCount);
        Assert.Equal(learningItemId, _studyRecordRepository.LastRequestedLearningItemId);
    }
}
