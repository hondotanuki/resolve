namespace Resolve.Infrastructure.External.Http;

public interface IExternalRequestClock
{
    DateTimeOffset UtcNow { get; }
    long GetTimestamp();
    TimeSpan GetElapsedTime(long start, long finish);
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}

public sealed class ExternalRequestClock : IExternalRequestClock
{
    private readonly TimeProvider _timeProvider;

    public ExternalRequestClock(TimeProvider timeProvider) => _timeProvider = timeProvider;

    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();

    public long GetTimestamp() => _timeProvider.GetTimestamp();

    public TimeSpan GetElapsedTime(long start, long finish) =>
        _timeProvider.GetElapsedTime(start, finish);

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        Task.Delay(delay, _timeProvider, cancellationToken);
}
