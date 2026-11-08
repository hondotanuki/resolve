using Resolve.Application.StudyRecords.List;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class ListStudyRecordsUseCaseTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);
    private readonly FakeLearningItemRepository _learningItems;
    private readonly FakeStudyRecordRepository _records;
    private readonly ListStudyRecordsUseCase _useCase;

    public ListStudyRecordsUseCaseTests()
    {
        _learningItems = new FakeLearningItemRepository();
        _records = new FakeStudyRecordRepository();
        _useCase = new ListStudyRecordsUseCase(_learningItems, _records);
    }

    [Fact]
    public async Task 対象が存在するとき指定したIDの履歴をOutputに変換する()
    {
        const int learningItemId = 42;
        _learningItems.GetByIdResult = new LearningItem(
            ItemType.EnglishWord,
            "apple",
            content: null,
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );
        var record = new StudyRecord(
            learningItemId,
            StudyResult.Failure,
            StudiedAt,
            durationSeconds: 120,
            memo: "復習する",
            targetSeconds: 600
        );
        _records.ListResult = [record];

        var output = await _useCase.ExecuteAsync(learningItemId);

        Assert.NotNull(output);
        var actual = Assert.Single(output);
        Assert.Equal(learningItemId, _learningItems.ReceivedId);
        Assert.Equal(learningItemId, _records.LastRequestedLearningItemId);
        Assert.Equal(1, _records.ListCallCount);
        Assert.Equal(record.Id, actual.Id);
        Assert.Equal(record.LearningItemId, actual.LearningItemId);
        Assert.Equal(record.Result, actual.Result);
        Assert.Equal(record.StudiedAt, actual.StudiedAt);
        Assert.Equal(record.DurationSeconds, actual.DurationSeconds);
        Assert.Equal(record.Memo, actual.Memo);
    }

    [Fact]
    public async Task 対象は存在するが記録がなければ空の一覧を返す()
    {
        _learningItems.GetByIdResult = new LearningItem(
            ItemType.EnglishWord,
            "apple",
            content: null,
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );

        var output = await _useCase.ExecuteAsync(42);

        Assert.NotNull(output);
        Assert.Empty(output);
        Assert.Equal(1, _records.ListCallCount);
        Assert.Equal(42, _records.LastRequestedLearningItemId);
    }

    [Fact]
    public async Task 対象が存在しなければnullを返し履歴を取得しない()
    {
        var output = await _useCase.ExecuteAsync(42);

        Assert.Null(output);
        Assert.Equal(42, _learningItems.ReceivedId);
        Assert.Equal(0, _records.ListCallCount);
    }
}
