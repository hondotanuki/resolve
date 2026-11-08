namespace Resolve.Api.Contracts;

public enum ApiValueSet
{
    StudyResults,
    ItemStatuses,
    ProblemSources,
    ContestCategories,
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ApiAllowedValuesAttribute(ApiValueSet valueSet) : Attribute
{
    public ApiValueSet ValueSet { get; } = valueSet;
}
