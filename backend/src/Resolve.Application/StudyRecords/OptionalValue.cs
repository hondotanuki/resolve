namespace Resolve.Application.StudyRecords;

public readonly record struct OptionalValue<T>(bool IsSpecified, T Value);

public static class OptionalValue
{
    public static OptionalValue<T> Of<T>(T value)
    {
        return new OptionalValue<T>(true, value);
    }
}
