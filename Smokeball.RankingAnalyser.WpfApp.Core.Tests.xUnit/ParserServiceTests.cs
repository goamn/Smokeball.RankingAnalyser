using Smokeball.RankingAnalyser.WpfApp.Core.Services;
using Xunit;

namespace Smokeball.RankingAnalyser.WpfApp.Core.Tests.xUnit;

public class ParserServiceTests
{
    private readonly ParserService _parserService;

    public ParserServiceTests()
    {
        _parserService = new ParserService();
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsCorrectRanking()
    {
        string htmlResponse = $"<html><body>{GenerateHtml(99, "another-site.com")}<div><h3>Test</h3><div>www.example.com</div></div></body></html>";
        string targetUrl = "www.example.com";

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);

        Assert.Equal([100], ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsMatchesWhenUrlIsMissingWww()
    {
        string targetUrl = "www.example.com";
        string targetUrlWithoutWww = "example.com";
        string htmlResponse = $"<html><body>{GenerateHtml(99, "random.com")}<div><h3>Test</h3><div>{targetUrl}</div></div></body></html>";

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrlWithoutWww);

        Assert.Equal([100], ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsNegativeOneWhenUrlIsFoundAbovePosition100()
    {
        string targetUrl = "www.new.website.super.com";
        string htmlResponse = $"<html><body>{GenerateHtml(150, "random.com")}<div><h3>Test</h3><div>{targetUrl}</div></div></body></html>";

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);

        Assert.Equal([-1], ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ThrowsExceptionWhenNoNodesMeetThreshold()
    {
        string htmlResponse = "<div><h3>Test</h3></div>"; // Insufficient nodes
        string targetUrl = "www.example.com";

        var exception = Assert.Throws<Exception>(() => _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl));
        Assert.Contains("The number of search result nodes is less than the expected amount", exception.Message);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ThrowsExceptionOnInvalidHtml()
    {
        string htmlResponse = "<div><h3>Test";
        string targetUrl = "www.example.com";

        var exception = Assert.Throws<Exception>(() => _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl));
        Assert.Contains("Error parsing HTML string from the response", exception.Message);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ThrowsExceptionWhenTooFewResults()
    {
        string targetUrl = "www.example.com";
        string htmlResponse = $"<body>{GenerateHtml(99, "only99.com")}</body>";

        var exception = Assert.Throws<Exception>(() => _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl));
        Assert.Contains("The number of search result nodes is less than the expected amount of: 100", exception.Message);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsNegativeOneWhenUrlNotFound()
    {
        string htmlResponse = $"<body>{GenerateHtml(105, "another-site.com")}</body>";
        string targetUrl = "www.example.com";

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);

        Assert.Equal([-1], ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_IgnoresNonSearchResultNodes()
    {
        string targetUrl = "www.example.com";
        string htmlResponse = "<body>"
            + $"<div role='button'><h3>Test</h3><div>{targetUrl}</div></div>"
            + $"<div><h3>Actual</h3><div>{targetUrl}</div></div>"
            + $"{GenerateHtml(100)}"
            + "</body>";

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);

        Assert.Equal([1], ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_IgnoresGoogleMapNode()
    {
        string targetUrl = "www.example.com";
        string htmlResponse = "<body>"
            + $"<div><h3>Test</h3><div>{targetUrl}</div><div>Directions</div></div>"
            + $"<div><h3>Actual</h3><div>{targetUrl}</div></div>"
            + $"{GenerateHtml(100)}"
            + "</body>";

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);

        Assert.Equal([1], ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_IgnoresNodesWithoutHeaderH3()
    {
        string targetUrl = "www.example.com";
        string htmlResponse = "<body>"
            + $"<div><h1>Test</h1><div>{targetUrl}</div><div>Directions</div></div>"
            + $"<div><h1>bad H1</h1><div>{targetUrl}</div></div>"
            + "<div><h3>Not the real one</h3><div>www.notthisone.com</div></div>"
            + "<div><h3>Not the real one</h3><div>www.notthisone.com</div></div>"
            + $"{GenerateHtml(100, targetUrl)}"
            + "</body>";
        List<int> expectedRanking = [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
            31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62,
            63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94,
            95, 96, 97, 98, 99, 100];

        var ranking = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);

        Assert.Equal(expectedRanking, ranking);
    }

    public static string GenerateHtml(int count, string site = "random.com")
    {
        string html = "";
        for (int i = 0; i < count; i++)
        {
            html += $"<div><h3>Test{i}</h3><div>{site}</div></div>";
        }
        return html;
    }
}
