using Resolve.Application.StudyRecords.List;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class ListStudyRecordsUseCaseTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task 対象が存在するとき指定したIDの履歴をOutputに変換する()
    {
        const int learningItemId = 42;
        var learningItems = new FakeLearningItemRepository
        {
            GetByIdResult = new LearningItem(ItemType.EnglishWord, "apple"),
        };
        var record = new StudyRecord(
            learningItemId,
            StudyResult.Failure,
            StudiedAt,
            durationSeconds: 120,
            memo: "復習する"
        );
        var records = new FakeStudyRecordRepository { ListResult = [record] };
        var useCase = new ListStudyRecordsUseCase(learningItems, records);

        var output = await useCase.ExecuteAsync(learningItemId);

        Assert.NotNull(output);
        var actual = Assert.Single(output);
        Assert.Equal(learningItemId, learningItems.ReceivedId);
        Assert.Equal(learningItemId, records.LastRequestedLearningItemId);
        Assert.Equal(1, records.ListCallCount);
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
        var learningItems = new FakeLearningItemRepository
        {
            GetByIdResult = new LearningItem(ItemType.EnglishWord, "apple"),
        };
        var records = new FakeStudyRecordRepository();
        var useCase = new ListStudyRecordsUseCase(learningItems, records);

        var output = await useCase.ExecuteAsync(42);

        Assert.NotNull(output);
        Assert.Empty(output);
        Assert.Equal(1, records.ListCallCount);
        Assert.Equal(42, records.LastRequestedLearningItemId);
    }

    [Fact]
    public async Task 対象が存在しなければnullを返し履歴を取得しない()
    {
        var learningItems = new FakeLearningItemRepository();
        var records = new FakeStudyRecordRepository();
        var useCase = new ListStudyRecordsUseCase(learningItems, records);

        var output = await useCase.ExecuteAsync(42);

        Assert.Null(output);
        Assert.Equal(42, learningItems.ReceivedId);
        Assert.Equal(0, records.ListCallCount);
    }
}
