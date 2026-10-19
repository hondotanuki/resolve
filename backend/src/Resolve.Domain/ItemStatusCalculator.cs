namespace Resolve.Domain;

public static class ItemStatusCalculator
{
    public static ItemStatus Calculate(IEnumerable<StudyRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var latestRecord = records.MaxBy(record => record.StudiedAt);

        if (latestRecord is null)
        {
            return ItemStatus.Unstarted;
        }

        return latestRecord.Result switch
        {
            StudyResult.Success => ItemStatus.Completed,
            StudyResult.Failure => ItemStatus.NeedsReview,
            StudyResult.Reviewed => ItemStatus.Completed,
            _ => throw new InvalidOperationException("Unknown study result."),
        };
    }
}
