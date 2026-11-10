using System.Net.Http.Json;
using System.Text;
using Resolve.Application.Interfaces;
using Resolve.Domain;

namespace Resolve.Api.Tests;

public sealed class TestProblemMetadataProvider : IProblemMetadataProvider
{
    public Uri Normalize(Uri uri)
    {
        if (
            !uri.IsAbsoluteUri
            || uri.Scheme != "https"
            || uri.Host != "atcoder.jp"
            || !uri.AbsolutePath.StartsWith(
                "/contests/abc482/tasks/test_",
                StringComparison.Ordinal
            )
        )
            throw new UnsupportedProblemUrlException("Unsupported test problem URL.");
        return new Uri($"https://atcoder.jp{uri.AbsolutePath}");
    }

    public Task<ProblemMetadata> GetAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(uri);
        var token = normalized.Segments[^1].Split('_')[1];
        var title = Encoding.UTF8.GetString(Convert.FromHexString(token));
        return Task.FromResult(
            new ProblemMetadata(
                normalized,
                title,
                ProblemSource.AtCoder,
                ContestCategory.Beginner,
                'A',
                "abc482",
                "ABC 482",
                new DateOnly(2026, 10, 31)
            )
        );
    }
}

public static class TestProblemApi
{
    public static Task<HttpResponseMessage> CreateAsync(
        HttpClient client,
        string title = "二分探索"
    )
    {
        var encoded = Convert.ToHexString(Encoding.UTF8.GetBytes(title));
        var url = $"https://atcoder.jp/contests/abc482/tasks/test_{encoded}_{Guid.NewGuid():N}";
        return client.PostAsJsonAsync("/learning-items", new { url });
    }
}
