using Resolve.Infrastructure.External;

namespace Resolve.Infrastructure.Tests;

public sealed class ProblemMetadataProviderTests : IDisposable
{
    private readonly ExternalHttpClient _client;
    private readonly AtCoderDatasetCache _cache;
    private readonly ProblemMetadataProvider _provider;

    public ProblemMetadataProviderTests()
    {
        _client = new ExternalHttpClient(
            new ExternalRequestClock(TimeProvider.System),
            new NeverSendHandler()
        );
        _cache = new AtCoderDatasetCache(
            _client,
            new ExternalRequestClock(TimeProvider.System),
            "unused/cache.json"
        );
        _provider = new ProblemMetadataProvider(
            new AtCoderProblemInfoProvider(_cache),
            new OmcProblemInfoProvider(_client)
        );
    }

    [Theory]
    [InlineData(
        "https://atcoder.jp/contests/abc482/tasks/abc482_a?lang=ja",
        "https://atcoder.jp/contests/abc482/tasks/abc482_a"
    )]
    [InlineData(
        "https://www.onlinemathcontest.com/contests/omcb087/tasks/5454",
        "https://onlinemathcontest.com/contests/omcb087/tasks/5454"
    )]
    public void 言語指定とホスト表記を正規化する(string input, string expected) =>
        Assert.Equal(expected, _provider.Normalize(new Uri(input)).AbsoluteUri);

    [Theory]
    [InlineData("https://atcoder.jp/contests/abc482/tasks/abc482_a?other=value")]
    [InlineData("http://atcoder.jp/contests/abc482/tasks/abc482_a")]
    [InlineData("https://example.com/contests/abc482/tasks/abc482_a")]
    public void 未対応URLは拒否する(string input) =>
        Assert.Throws<ArgumentException>(() => _provider.Normalize(new Uri(input)));

    public void Dispose()
    {
        _cache.Dispose();
        _client.Dispose();
    }

    private sealed class NeverSendHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => throw new InvalidOperationException("No network requests are expected.");
    }
}
