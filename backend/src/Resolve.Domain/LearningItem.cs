namespace Resolve.Domain;

public class LearningItem
{
    public int Id { get; private set; }
    public ItemType ItemType { get; private set; }
    public string Title { get; private set; }
    public string? Content { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public LearningItem(ItemType itemType, string title, string? content = null)
    {
        if (!Enum.IsDefined(itemType))
            throw new ArgumentOutOfRangeException(nameof(itemType));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be empty.", nameof(title));

        ItemType = itemType;
        Title = title;
        Content = content;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
