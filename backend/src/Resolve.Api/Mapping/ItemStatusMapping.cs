using Resolve.Api.Contracts;
using Resolve.Domain;

namespace Resolve.Api.Mapping;

public static class ItemStatusMapping
{
    public static string ToApiValue(ItemStatus itemStatus)
    {
        return itemStatus switch
        {
            ItemStatus.Unstarted => ApiValues.ItemStatuses.Unstarted,
            ItemStatus.NeedsReview => ApiValues.ItemStatuses.NeedsReview,
            ItemStatus.Completed => ApiValues.ItemStatuses.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(itemStatus), itemStatus, null),
        };
    }
}
