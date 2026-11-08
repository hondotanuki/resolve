using Resolve.Application.StudyRecords.Create;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class CreateStudyRecordUseCaseTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
    private readonly FakeLearningItemRepository _learningItemRepository;
    private readonly FakeStudyRecordRepository _studyRecordRepository;
    private readonly CreateStudyRecordUseCase _useCase;

    public CreateStudyRecordUseCaseTests()
    {
        _learningItemRepository = new FakeLearningItemRepository();
        _studyRecordRepository = new FakeStudyRecordRepository();
        _useCase = new CreateStudyRecordUseCase(_learningItemRepository, _studyRecordRepository);
    }

    private static LearningItem CreateItem()
    {
        var item = new LearningItem(
            "二分探索",
            source: ProblemSource.AtCoder,
            category: ContestCategory.Beginner,
            problemIndex: 'A'
        );
        item.SetProblemCategory(ProblemSource.AtCoder, ContestCategory.Beginner, 'A');
        return item;
    }

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

    [Fact]
    public async Task 正常な入力で記録を保存して現在状態を返す()
    {
        // Arrange
        _learningItemRepository.GetByIdResult = CreateItem();

        // 保存結果。入力とは異なるMemo・DurationSecondsを設定する。
        StudyRecord savedRecord = new(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1200,
            targetSeconds: 600,
            memo: "保存されたメモ"
        );

        // 過去の記録を追加した場合、最新記録は別に存在する。
        StudyRecord latestRecord = new(1, StudyResult.Failure, StudiedAt.AddDays(3), 0, 600);

        _studyRecordRepository.CreateResult = savedRecord;
        _studyRecordRepository.LatestRecord = latestRecord;
        CreateStudyRecordInput input = CreateInput();

        // Act
        CreateStudyRecordOutput output = Assert.IsType<CreateStudyRecordOutput>(
            await _useCase.ExecuteAsync(input)
        );

        // Assert: 保存処理
        Assert.Equal(1, _studyRecordRepository.CreateCallCount);

        StudyRecord received = Assert.IsType<StudyRecord>(_studyRecordRepository.ReceivedRecord);

        Assert.Equal(input.LearningItemId, received.LearningItemId);
        Assert.Equal(input.Result, received.Result);
        Assert.Equal(input.StudiedAt, received.StudiedAt);
        Assert.Equal(input.DurationSeconds, received.DurationSeconds);
        Assert.Equal(input.Memo, received.Memo);

        // Assert: 最新記録の取得
        Assert.Equal(1, _studyRecordRepository.GetLatestCallCount);
        Assert.Equal(1, _studyRecordRepository.LastRequestedLearningItemId);

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
        CreateStudyRecordOutput? output = await _useCase.ExecuteAsync(CreateInput());

        Assert.Null(output);
        Assert.Equal(0, _studyRecordRepository.CreateCallCount);
        Assert.Equal(0, _studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task 不正なStudyResultなら保存しない()
    {
        _learningItemRepository.GetByIdResult = CreateItem();

        CreateStudyRecordInput input = CreateInput() with { Result = (StudyResult)999 };

        _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _useCase.ExecuteAsync(input)
        );

        Assert.Equal(0, _studyRecordRepository.CreateCallCount);
        Assert.Equal(0, _studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task 負のDurationSecondsなら保存しない()
    {
        _learningItemRepository.GetByIdResult = CreateItem();

        CreateStudyRecordInput input = CreateInput() with { DurationSeconds = -1 };

        _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _useCase.ExecuteAsync(input)
        );

        Assert.Equal(0, _studyRecordRepository.CreateCallCount);
        Assert.Equal(0, _studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task StudiedAtが未設定なら保存しない()
    {
        _learningItemRepository.GetByIdResult = CreateItem();

        CreateStudyRecordInput input = CreateInput() with { StudiedAt = default };

        _ = await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(input));

        Assert.Equal(0, _studyRecordRepository.CreateCallCount);
        Assert.Equal(0, _studyRecordRepository.GetLatestCallCount);
    }

    [Fact]
    public async Task Memoは省略できる()
    {
        _learningItemRepository.GetByIdResult = CreateItem();

        StudyRecord savedRecord = new(1, StudyResult.Success, StudiedAt, 0, 600);

        _studyRecordRepository.CreateResult = savedRecord;
        _studyRecordRepository.LatestRecord = savedRecord;

        CreateStudyRecordInput input = CreateInput() with { DurationSeconds = 0, Memo = null };

        CreateStudyRecordOutput output = Assert.IsType<CreateStudyRecordOutput>(
            await _useCase.ExecuteAsync(input)
        );

        Assert.Equal(0, output.DurationSeconds);
        Assert.Null(output.Memo);
        Assert.Equal(ItemStatus.Completed, output.Status);
        Assert.Equal(1, _studyRecordRepository.CreateCallCount);
    }
}
