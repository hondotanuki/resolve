namespace Resolve.Application.Interfaces;

public sealed class UnsupportedProblemCategoryException(string message) : Exception(message);
