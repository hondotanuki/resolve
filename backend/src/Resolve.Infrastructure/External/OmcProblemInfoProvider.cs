using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Resolve.Domain;

namespace Resolve.Infrastructure.External;

public sealed partial class OmcProblemInfoProvider(ExternalHttpClient client)
{
    public async Task<OmcProblemInfo> GetAsync(
        Uri uri,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (
            !uri.IsAbsoluteUri
            || uri.Scheme != Uri.UriSchemeHttps
            || uri.IdnHost is not ("onlinemathcontest.com" or "www.onlinemathcontest.com")
            || !uri.IsDefaultPort
            || uri.UserInfo.Length != 0
            || uri.Query.Length != 0
            || uri.Fragment.Length != 0
        )
            throw new ArgumentException("Unsupported OMC problem URL.", nameof(uri));

        var path = ProblemPath().Match(uri.AbsolutePath);
        if (!path.Success)
            throw new ArgumentException(
                "The URL must identify an OMC, OMCB, or OMCE problem.",
                nameof(uri)
            );

        var contestId = path.Groups[1].Value;
        var problemUrl = new Uri($"https://onlinemathcontest.com{uri.AbsolutePath}");
        var contestUrl = new Uri($"https://onlinemathcontest.com/contests/{contestId}");
        var parser = new HtmlParser();
        using var problemResponse = await client.GetAsync(problemUrl, cancellationToken);
        problemResponse.EnsureSuccessStatusCode();
        EnsureExpectedPath(problemResponse, problemUrl);
        var problemHtml = await problemResponse.Content.ReadAsStringAsync(cancellationToken);
        using var problemDocument = await parser.ParseDocumentAsync(problemHtml, cancellationToken);
        var headings = problemDocument.QuerySelectorAll("h1#problem-title");
        if (headings.Length != 1)
            throw new InvalidDataException(
                "The OMC problem page does not contain a unique problem title."
            );
        var title = headings[0].TextContent.Trim();
        var titleMatch = ProblemTitle().Match(title);
        if (
            !titleMatch.Success
            || !string.Equals(
                titleMatch.Groups[1].Value,
                contestId,
                StringComparison.OrdinalIgnoreCase
            )
        )
            throw new InvalidDataException(
                "The OMC problem title does not match the requested contest."
            );
        var index = titleMatch.Groups[2].Value[0];
        var category = contestId switch
        {
            var id when id.StartsWith("omcb", StringComparison.Ordinal) => ContestCategory.Beginner,
            var id when id.StartsWith("omce", StringComparison.Ordinal) => ContestCategory.Expert,
            _ => ContestCategory.Regular,
        };
        if (TargetTime.GetSeconds(ProblemSource.OnlineMathContest, category, index) is null)
            throw new InvalidOperationException(
                "No target time is defined for the OMC problem category."
            );

        using var contestResponse = await client.GetAsync(contestUrl, cancellationToken);
        contestResponse.EnsureSuccessStatusCode();
        EnsureExpectedPath(contestResponse, contestUrl);
        var contestHtml = await contestResponse.Content.ReadAsStringAsync(cancellationToken);
        using var contestDocument = await parser.ParseDocumentAsync(contestHtml, cancellationToken);
        var contestName = contestId.ToUpperInvariant();
        if (!contestDocument.QuerySelectorAll("h1").Any(x => x.TextContent.Trim() == contestName))
            throw new InvalidDataException(
                "The OMC contest page does not match the requested contest."
            );
        var startLabels = contestDocument
            .QuerySelectorAll("p.list-group-item-text")
            .Where(x => x.TextContent.Trim() == "Begin Time")
            .ToArray();
        var heading = startLabels.Length == 1 ? startLabels[0].PreviousElementSibling : null;
        var displayedDate =
            heading is not null && heading.ClassList.Contains("list-group-item-heading")
                ? heading
                    .TextContent.Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()
                : null;
        if (
            !DateOnly.TryParseExact(
                displayedDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var startDate
            )
        )
            throw new InvalidDataException(
                "The OMC contest page does not contain a valid displayed start date."
            );

        return new OmcProblemInfo(
            problemUrl,
            title,
            contestId,
            contestName,
            startDate,
            category,
            index
        );
    }

    private static void EnsureExpectedPath(HttpResponseMessage response, Uri expected)
    {
        var actual = response.RequestMessage?.RequestUri;
        if (
            actual is null
            || actual.Scheme != Uri.UriSchemeHttps
            || actual.IdnHost is not ("onlinemathcontest.com" or "www.onlinemathcontest.com")
            || actual.AbsolutePath != expected.AbsolutePath
        )
            throw new InvalidDataException("OMC redirected to a different page.");
    }

    [GeneratedRegex(
        @"^/contests/((?:omcb|omce|omc)[0-9]+)/tasks/([0-9]+)$",
        RegexOptions.CultureInvariant
    )]
    private static partial Regex ProblemPath();

    [GeneratedRegex(@"^(OMC(?:B|E)?[0-9]+)\(([A-Z])\)$", RegexOptions.CultureInvariant)]
    private static partial Regex ProblemTitle();
}
