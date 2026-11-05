using Microsoft.EntityFrameworkCore;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Infrastructure.Persistence.Repositories;

public sealed class EfStudyRecordRepository : IStudyRecordRepository
{
    private readonly ResolveDbContext _dbContext;

    public EfStudyRecordRepository(ResolveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StudyRecord> CreateAsync(
        StudyRecord studyRecord,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.StudyRecords.Add(studyRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return studyRecord;
    }

    public async Task<StudyRecord?> GetByIdAsync(
        int studyRecordId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .StudyRecords.AsTracking()
            .FirstOrDefaultAsync(record => record.Id == studyRecordId, cancellationToken);
    }

    public async Task UpdateAsync(
        StudyRecord studyRecord,
        CancellationToken cancellationToken = default
    )
    {
        if (_dbContext.Entry(studyRecord).State == EntityState.Detached)
        {
            throw new InvalidOperationException("StudyRecord must be tracked.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        StudyRecord studyRecord,
        CancellationToken cancellationToken = default
    )
    {
        _dbContext.StudyRecords.Remove(studyRecord);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<StudyRecord?> GetLatestByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .StudyRecords.AsNoTracking()
            .Where(record => record.LearningItemId == learningItemId)
            .OrderByDescending(record => record.StudiedAt)
            .ThenByDescending(record => record.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudyRecord>> ListByLearningItemIdAsync(
        int learningItemId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .StudyRecords.AsNoTracking()
            .Where(record => record.LearningItemId == learningItemId)
            .OrderByDescending(record => record.StudiedAt)
            .ThenByDescending(record => record.Id)
            .ToListAsync(cancellationToken);
    }
}
