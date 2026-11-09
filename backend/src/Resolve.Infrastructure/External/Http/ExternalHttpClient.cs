using System.Net;

namespace Resolve.Infrastructure.External.Http;

// This client is shared across requests so all calls to a provider use the same gate.
public sealed class ExternalHttpClient : IDisposable
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(2);
    private const int MaxRedirects = 2;
    private readonly HttpClient _client;
    private readonly IExternalRequestClock _clock;
    private readonly ProviderState _atCoder = new();
    private readonly ProviderState _atCoderProblems = new();
    private readonly ProviderState _omc = new();

    public ExternalHttpClient(IExternalRequestClock clock, HttpMessageHandler handler)
    {
        _clock = clock;
        _client = new HttpClient(handler);
    }

    public async Task<HttpResponseMessage> GetAsync(
        Uri uri,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uri);
        for (var redirects = 0; ; redirects++)
        {
            var state = GetProvider(uri);
            var response = await SendAsync(uri, state, cancellationToken);
            if (!IsRedirect(response.StatusCode))
                return response;

            try
            {
                if (redirects >= MaxRedirects)
                    throw new HttpRequestException("リダイレクトの回数が上限を超えました。");
                var location =
                    response.Headers.Location
                    ?? throw new HttpRequestException(
                        "リダイレクト先を示すLocationヘッダーがありません。"
                    );
                uri = location.IsAbsoluteUri ? location : new Uri(uri, location);
                _ = GetProvider(uri);
            }
            finally
            {
                response.Dispose();
            }
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        Uri uri,
        ProviderState state,
        CancellationToken cancellationToken
    )
    {
        await state.Gate.WaitAsync(cancellationToken);
        try
        {
            while (true)
            {
                var now = _clock.GetTimestamp();
                var intervalWait = state.LastStart is long start
                    ? MinimumInterval - _clock.GetElapsedTime(start, now)
                    : TimeSpan.Zero;
                var retryWait = state.RetryStart is long retryStart
                    ? state.RetryDelay - _clock.GetElapsedTime(retryStart, now)
                    : TimeSpan.Zero;
                var wait = intervalWait > retryWait ? intervalWait : retryWait;
                if (wait <= TimeSpan.Zero)
                    break;
                await _clock.DelayAsync(
                    wait > TimeSpan.FromDays(1) ? TimeSpan.FromDays(1) : wait,
                    cancellationToken
                );
            }

            cancellationToken.ThrowIfCancellationRequested();
            state.LastStart = _clock.GetTimestamp();
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await _client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            var retryAfter = response.Headers.RetryAfter;
            if (retryAfter is not null)
            {
                var delay = retryAfter.Delta ?? (retryAfter.Date - _clock.UtcNow) ?? TimeSpan.Zero;
                if (delay > TimeSpan.Zero)
                {
                    state.RetryStart = _clock.GetTimestamp();
                    state.RetryDelay = delay;
                }
            }
            return response;
        }
        finally
        {
            state.Gate.Release();
        }
    }

    private ProviderState GetProvider(Uri uri)
    {
        if (
            !uri.IsAbsoluteUri
            || uri.Scheme != Uri.UriSchemeHttps
            || !uri.IsDefaultPort
            || uri.UserInfo.Length != 0
        )
            throw new ArgumentException("許可されたHTTPSの送信先を指定してください。", nameof(uri));

        return uri.IdnHost.ToLowerInvariant() switch
        {
            "atcoder.jp" => _atCoder,
            "kenkoooo.com" => _atCoderProblems,
            "onlinemathcontest.com" or "www.onlinemathcontest.com" => _omc,
            _ => throw new ArgumentException("許可されていない送信先です。", nameof(uri)),
        };
    }

    private static bool IsRedirect(HttpStatusCode status) =>
        status
            is HttpStatusCode.MovedPermanently
                or HttpStatusCode.Redirect
                or HttpStatusCode.SeeOther
                or HttpStatusCode.TemporaryRedirect
                or HttpStatusCode.PermanentRedirect;

    public void Dispose()
    {
        _client.Dispose();
        _atCoder.Gate.Dispose();
        _atCoderProblems.Gate.Dispose();
        _omc.Gate.Dispose();
    }

    private sealed class ProviderState
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);
        public long? LastStart { get; set; }
        public long? RetryStart { get; set; }
        public TimeSpan RetryDelay { get; set; }
    }
}
