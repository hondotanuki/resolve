namespace Resolve.Domain;

public class LearningItem
{
    public int Id { get; private set; }
    public ProblemSource Source { get; private set; }
    public ContestCategory Category { get; private set; }
    public char ProblemIndex { get; private set; }
    public string Title { get; private set; }
    public string ProblemUrl { get; private set; }
    public string ContestId { get; private set; }
    public string ContestName { get; private set; }
    public DateOnly ContestStartDate { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public LearningItem(
        string title,
        ProblemSource source,
        ContestCategory category,
        char problemIndex,
        string problemUrl,
        string contestId,
        string contestName,
        DateOnly contestStartDate
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (TargetTime.GetSeconds(source, category, problemIndex) is null)
            throw new ArgumentException("対応していない問題区分です。");
        ArgumentException.ThrowIfNullOrWhiteSpace(problemUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(contestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contestName);
        if (contestStartDate == default)
            throw new ArgumentException("コンテストの開始日は必須です。", nameof(contestStartDate));
        Title = title;
        Source = source;
        Category = category;
        ProblemIndex = problemIndex;
        ProblemUrl = problemUrl;
        ContestId = contestId;
        ContestName = contestName;
        ContestStartDate = contestStartDate;
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
            throw new ArgumentException("対応していない問題区分です。");

        Source = source;
        Category = category;
        ProblemIndex = problemIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("タイトルを入力してください。", nameof(title));

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }
}
