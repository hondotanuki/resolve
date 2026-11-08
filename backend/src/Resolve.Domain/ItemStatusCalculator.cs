namespace Resolve.Domain;

public static class ItemStatusCalculator
{
    public static ItemStatus Calculate(IEnumerable<StudyRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var latestRecord = records
            .OrderByDescending(record => record.StudiedAt)
            .ThenByDescending(record => record.Id)
            .FirstOrDefault();

        if (latestRecord is null)
        {
            return ItemStatus.Unstarted;
        }

        return latestRecord.Result switch
        {
            StudyResult.Success => latestRecord.IsOvertime
                ? ItemStatus.NeedsReview
                : ItemStatus.Completed,
            StudyResult.Failure => ItemStatus.NeedsReview,
            _ => throw new InvalidOperationException("Unknown study result."),
        };
    }
}
