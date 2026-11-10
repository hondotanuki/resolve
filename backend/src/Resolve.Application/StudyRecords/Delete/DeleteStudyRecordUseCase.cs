using Resolve.Application.Interfaces;

namespace Resolve.Application.StudyRecords.Delete;

public sealed class DeleteStudyRecordUseCase
{
    private readonly IStudyRecordRepository _repository;

    public DeleteStudyRecordUseCase(IStudyRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteStudyRecordOutput> ExecuteAsync(
        DeleteStudyRecordInput input,
        CancellationToken cancellationToken = default
    )
    {
        var record = await _repository.GetByIdAsync(input.StudyRecordId, cancellationToken);

        if (record is null)
        {
            return new(false);
        }

        await _repository.DeleteAsync(record, cancellationToken);

        return new(true);
    }
}
