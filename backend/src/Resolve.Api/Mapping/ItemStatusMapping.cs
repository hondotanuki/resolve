using Resolve.Domain;

namespace Resolve.Api.Mapping;

public static class ItemStatusMapping
{
    public static string ToApiValue(ItemStatus itemStatus)
    {
        return itemStatus switch
        {
            ItemStatus.Unstarted => "unstarted",
            ItemStatus.NeedsReview => "needs_review",
            ItemStatus.Completed => "completed",
            _ => throw new ArgumentOutOfRangeException(nameof(itemStatus), itemStatus, null),
        };
    }
}
