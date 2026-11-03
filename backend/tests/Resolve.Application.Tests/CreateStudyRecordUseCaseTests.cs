using Resolve.Application.StudyRecords.Create;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class CreateStudyRecordUseCaseTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);

    private static CreateStudyRecordInput CreateInput()
    {
        return new CreateStudyRecordInput
        {
            LearningItemId = 1,
            Result = StudyResult.Success,
            StudiedAt = StudiedAt,
            DurationSeconds = 1800,
            Memo = "二分探索を学習した",
        };
    }

    private static FakeLearningItemRepository CreateLearningItemRepository()
    {
        return new FakeLearningItemRepository
        {
            GetByIdResult = new LearningItem(
                ItemType.AlgorithmProblem,
                "二分探索",
                "二分探索を解く"
            ),
        };
    }

    [Fact]
    public async Task 正常な入力で記録を保存して現在状態を返す()
    {
        // Arrange
        FakeLearningItemRepository learningItemRepository = CreateLearningItemRepository();

        // 保存結果。入力とは異なるMemo・DurationSecondsを設定する。
        StudyRecord savedRecord = new(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1200,
            memo: "保存されたメモ"
        );

        // 過去の記録を追加した場合、最新記録は別に存在する。
        StudyRecord latestRecord = new(1, StudyResult.Failure, StudiedAt.AddDays(3));

        FakeStudyRecordRepository studyRecordRepository = new()
        {
            CreateResult = savedRecord,
            LatestRecord = latestRecord,
        };

        CreateStudyRecordUseCase useCase = new(learningItemRepository, studyRecordRepository);
        CreateStudyRecordInput input = CreateInput();

        // Act
        CreateStudyRecordOutput output = Assert.IsType<CreateStudyRecordOutput>(
            await useCase.ExecuteAsync(input)
        );

        // Assert: 保存処理
        Assert.Equal(1, studyRecordRepository.CreateCallCount);

        StudyRecord received = Assert.IsType<StudyRecord>(studyRecordRepository.ReceivedRecord);

        Assert.Equal(input.LearningItemId, received.LearningItemId);
        Assert.Equal(input.Result, received.Result);
        Assert.Equal(input.StudiedAt, received.StudiedAt);
        Assert.Equal(input.DurationSeconds, received.DurationSeconds);
        Assert.Equal(input.Memo, received.Memo);

        // Assert: 最新記録の取得
        Assert.Equal(1, studyRecordRepository.GetLatestCallCount);
        Assert.Equal(1, studyRecordRepository.LastRequestedLearningItemId);

        // Assert: Outputには保存結果を反映
        Assert.Equal(savedRecord.Id, output.Id);
        Assert.Equal(savedRecord.LearningItemId, output.LearningItemId);
        Assert.Equal(savedRecord.Result, output.Result);
        Assert.Equal(savedRecord.StudiedAt, output.StudiedAt);
        Assert.Equal(savedRecord.DurationSeconds, output.DurationSeconds);
        Assert.Equal(savedRecord.Memo, output.Memo);
        Assert.Equal(savedRecord.CreatedAt, output.CreatedAt);
        Assert.Equal(savedRecord.UpdatedAt, output.UpdatedAt);

        // Assert: Output.Statusは最新記録から算出
        Assert.Equal(ItemStatus.NeedsReview, output.Status);
    }

    [Fact]
    public async Task LearningItemが存在しなければnullを返して保存しない()
    {
        FakeLearningItemRepository learningItemRepository = new() { GetByIdResult = null };
        FakeStudyRecordRepository studyRecordRepository = new();

        CreateStudyRecordUseCase useCase = new(learningItemRepository, studyRecordRepository);

        CreateStudyRecordOutput? output = await useCase.ExecuteAsync(CreateInput());

        Assert.Null(output);
        Assert.Equal(0, studyRecordRepository.CreateCallCount);
        Assert.Equal(0, studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task 不正なStudyResultなら保存しない()
    {
        FakeLearningItemRepository learningItemRepository = CreateLearningItemRepository();
        FakeStudyRecordRepository studyRecordRepository = new();

        CreateStudyRecordUseCase useCase = new(learningItemRepository, studyRecordRepository);

        CreateStudyRecordInput input = CreateInput() with { Result = (StudyResult)999 };

        _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            useCase.ExecuteAsync(input)
        );

        Assert.Equal(0, studyRecordRepository.CreateCallCount);
        Assert.Equal(0, studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task 負のDurationSecondsなら保存しない()
    {
        FakeLearningItemRepository learningItemRepository = CreateLearningItemRepository();
        FakeStudyRecordRepository studyRecordRepository = new();

        CreateStudyRecordUseCase useCase = new(learningItemRepository, studyRecordRepository);

        CreateStudyRecordInput input = CreateInput() with { DurationSeconds = -1 };

        _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            useCase.ExecuteAsync(input)
        );

        Assert.Equal(0, studyRecordRepository.CreateCallCount);
        Assert.Equal(0, studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task StudiedAtが未設定なら保存しない()
    {
        FakeLearningItemRepository learningItemRepository = CreateLearningItemRepository();
        FakeStudyRecordRepository studyRecordRepository = new();

        CreateStudyRecordUseCase useCase = new(learningItemRepository, studyRecordRepository);

        CreateStudyRecordInput input = CreateInput() with { StudiedAt = default };

        _ = await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(input));

        Assert.Equal(0, studyRecordRepository.CreateCallCount);
        Assert.Equal(0, studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task DurationSecondsとMemoは省略できる()
    {
        FakeLearningItemRepository learningItemRepository = CreateLearningItemRepository();

        StudyRecord savedRecord = new(1, StudyResult.Success, StudiedAt);

        FakeStudyRecordRepository studyRecordRepository = new()
        {
            CreateResult = savedRecord,
            LatestRecord = savedRecord,
        };

        CreateStudyRecordUseCase useCase = new(learningItemRepository, studyRecordRepository);

        CreateStudyRecordInput input = CreateInput() with { DurationSeconds = null, Memo = null };

        CreateStudyRecordOutput output = Assert.IsType<CreateStudyRecordOutput>(
            await useCase.ExecuteAsync(input)
        );

        Assert.Null(output.DurationSeconds);
        Assert.Null(output.Memo);
        Assert.Equal(ItemStatus.Completed, output.Status);
        Assert.Equal(1, studyRecordRepository.CreateCallCount);
    }
}
