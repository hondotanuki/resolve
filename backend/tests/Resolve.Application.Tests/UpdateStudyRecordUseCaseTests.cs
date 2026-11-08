using Resolve.Application.StudyRecords;
using Resolve.Application.StudyRecords.Update;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class UpdateStudyRecordUseCaseTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
    private readonly FakeStudyRecordRepository _repository;
    private readonly UpdateStudyRecordUseCase _useCase;

    public UpdateStudyRecordUseCaseTests()
    {
        _repository = new FakeStudyRecordRepository();
        _useCase = new UpdateStudyRecordUseCase(_repository);
    }

    [Fact]
    public async Task 指定した項目だけ訂正できる()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前",
            targetSeconds: 600
        );
        _repository.RecordById = record;
        var originalCreatedAt = record.CreatedAt;

        var updatedStudiedAt = StudiedAt.AddDays(1);
        var input = new UpdateStudyRecordInput
        {
            Id = 10,
            Result = default,
            StudiedAt = OptionalValue.Of(updatedStudiedAt),
            DurationSeconds = default,
            Memo = OptionalValue.Of<string?>("訂正後"),
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.NotNull(output);
        Assert.Equal(updatedStudiedAt, output.StudiedAt);
        Assert.Equal(StudyResult.Success, output.Result);
        Assert.Equal(1800, output.DurationSeconds);
        Assert.Equal("訂正後", output.Memo);

        Assert.Equal(record.Id, output.Id);
        Assert.Equal(1, output.LearningItemId);
        Assert.Equal(originalCreatedAt, output.CreatedAt);

        Assert.Equal(1, _repository.GetByIdCallCount);
        Assert.Equal(10, _repository.LastRequestedStudyRecordId);
        Assert.Equal(1, _repository.UpdateCallCount);
        Assert.Same(record, _repository.ReceivedUpdatedRecord);
    }

    [Fact]
    public async Task 明示的なnullで値を消去できる()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前",
            targetSeconds: 600
        );
        _repository.RecordById = record;

        var input = new UpdateStudyRecordInput
        {
            Id = 10,
            Result = default,
            StudiedAt = default,
            DurationSeconds = default,
            Memo = OptionalValue.Of<string?>(null),
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.NotNull(output);
        Assert.Equal(1800, output.DurationSeconds);
        Assert.Null(output.Memo);
        Assert.Equal(StudiedAt, output.StudiedAt);
        Assert.Equal(StudyResult.Success, output.Result);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task 対象が存在しなければ訂正しない()
    {
        var input = new UpdateStudyRecordInput
        {
            Id = 999,
            Result = default,
            StudiedAt = default,
            DurationSeconds = default,
            Memo = default,
        };

        var output = await _useCase.ExecuteAsync(input);

        Assert.Null(output);
        Assert.Equal(1, _repository.GetByIdCallCount);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task 不正な訂正では保存しない()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 1800,
            memo: "訂正前",
            targetSeconds: 600
        );
        _repository.RecordById = record;

        var input = new UpdateStudyRecordInput
        {
            Id = 10,
            Result = default,
            StudiedAt = default,
            DurationSeconds = OptionalValue.Of(-1),
            Memo = OptionalValue.Of<string?>("訂正後"),
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _useCase.ExecuteAsync(input));

        Assert.Equal(0, _repository.UpdateCallCount);
        Assert.Equal(1800, record.DurationSeconds);
        Assert.Equal("訂正前", record.Memo);
    }
}
