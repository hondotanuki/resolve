using Resolve.Application.Interfaces;

namespace Resolve.Application.StudyRecords.Delete;

public sealed class DeleteStudyRecordUseCase
{
    private readonly IStudyRecordRepository _repository;

    public DeleteStudyRecordUseCase(IStudyRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(
        int studyRecordId,
        CancellationToken cancellationToken = default
    )
    {
        var record = await _repository.GetByIdAsync(studyRecordId, cancellationToken);

        if (record is null)
        {
            return false;
        }

        await _repository.DeleteAsync(record, cancellationToken);

        return true;
    }
}
