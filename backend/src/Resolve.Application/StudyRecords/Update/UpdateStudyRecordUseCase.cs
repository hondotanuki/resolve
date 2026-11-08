using Resolve.Application.Interfaces;

namespace Resolve.Application.StudyRecords.Update;

public sealed class UpdateStudyRecordUseCase
{
    private readonly IStudyRecordRepository _repository;

    public UpdateStudyRecordUseCase(IStudyRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateStudyRecordOutput?> ExecuteAsync(
        UpdateStudyRecordInput input,
        CancellationToken cancellationToken = default
    )
    {
        var record = await _repository.GetByIdAsync(input.Id, cancellationToken);

        if (record is null)
        {
            return null;
        }

        // 入力が指定されていれば更新、されていなければ既存の値を使用する
        var studiedAt = input.StudiedAt.IsSpecified ? input.StudiedAt.Value : record.StudiedAt;
        var result = input.Result.IsSpecified ? input.Result.Value : record.Result;
        var durationSeconds = input.DurationSeconds.IsSpecified
            ? input.DurationSeconds.Value
            : record.DurationSeconds;
        var memo = input.Memo.IsSpecified ? input.Memo.Value : record.Memo;

        record.Update(result, studiedAt, durationSeconds, memo);

        await _repository.UpdateAsync(record, cancellationToken);

        return new UpdateStudyRecordOutput(
            record.Id,
            record.LearningItemId,
            record.Result,
            record.StudiedAt,
            record.DurationSeconds,
            record.TargetSeconds,
            record.Memo,
            record.CreatedAt,
            record.UpdatedAt
        );
    }
}
