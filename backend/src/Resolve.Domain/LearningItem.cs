namespace Resolve.Domain;

public class LearningItem
{
    public int Id { get; private set; }
    public ProblemSource Source { get; private set; }
    public ContestCategory Category { get; private set; }
    public char ProblemIndex { get; private set; }
    public string Title { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public LearningItem(
        string title,
        ProblemSource source,
        ContestCategory category,
        char problemIndex
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title must not be empty.", nameof(title));

        if (TargetTime.GetSeconds(source, category, problemIndex) is null)
            throw new ArgumentException("Unsupported problem category.");

        Title = title;
        Source = source;
        Category = category;
        ProblemIndex = problemIndex;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void SetProblemCategory(
        ProblemSource source,
        ContestCategory category,
        char problemIndex
    )
    {
        if (TargetTime.GetSeconds(source, category, problemIndex) is null)
            throw new ArgumentException("Unsupported problem category.");

        Source = source;
        Category = category;
        ProblemIndex = problemIndex;
        UpdatedAt = DateTime.UtcNow;
    }
}
