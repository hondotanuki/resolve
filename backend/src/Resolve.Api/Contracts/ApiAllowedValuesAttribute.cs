namespace Resolve.Api.Contracts;

public enum ApiValueSet
{
    ItemTypes,
    StudyResults,
    ItemStatuses,
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ApiAllowedValuesAttribute(ApiValueSet valueSet) : Attribute
{
    public ApiValueSet ValueSet { get; } = valueSet;
}
