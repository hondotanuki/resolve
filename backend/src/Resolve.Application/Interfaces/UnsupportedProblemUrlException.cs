namespace Resolve.Application.Interfaces;

public sealed class UnsupportedProblemUrlException(string message) : ArgumentException(message);
