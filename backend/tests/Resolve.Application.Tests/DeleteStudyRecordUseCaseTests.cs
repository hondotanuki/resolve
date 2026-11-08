using Resolve.Application.StudyRecords.Delete;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class DeleteStudyRecordUseCaseTests
{
    private static readonly DateTime StudiedAt = new(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
    private readonly FakeStudyRecordRepository _repository;
    private readonly DeleteStudyRecordUseCase _useCase;

    public DeleteStudyRecordUseCaseTests()
    {
        _repository = new FakeStudyRecordRepository();
        _useCase = new DeleteStudyRecordUseCase(_repository);
    }

    [Fact]
    public async Task 対象が存在すれば削除できる()
    {
        var record = new StudyRecord(
            1,
            StudyResult.Success,
            StudiedAt,
            durationSeconds: 0,
            targetSeconds: 600
        );
        _repository.RecordById = record;

        var result = await _useCase.ExecuteAsync(10);

        Assert.True(result);
        Assert.Equal(1, _repository.GetByIdCallCount);
        Assert.Equal(10, _repository.LastRequestedStudyRecordId);
        Assert.Equal(1, _repository.DeleteCallCount);
        Assert.Same(record, _repository.ReceivedDeletedRecord);
    }

    [Fact]
    public async Task 対象が存在しなければ削除しない()
    {
        var result = await _useCase.ExecuteAsync(999);

        Assert.False(result);
        Assert.Equal(1, _repository.GetByIdCallCount);
        Assert.Equal(0, _repository.DeleteCallCount);
    }
}
