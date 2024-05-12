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

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl);

        Assert.Equal(100, ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsMatchesWhenUrlIsMissingWww()
    {
        string targetUrl = "www.example.com";
        string targetUrlWithoutWww = "example.com";
        string htmlResponse = $"<html><body>{GenerateHtml(99, "random.com")}<div><h3>Test</h3><div>{targetUrl}</div></div></body></html>";

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrlWithoutWww);

        Assert.Equal(100, ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsNegativeOneWhenUrlIsFoundAbovePosition100()
    {
        string targetUrl = "www.new.website.super.com";
        string htmlResponse = $"<html><body>{GenerateHtml(150, "random.com")}<div><h3>Test</h3><div>{targetUrl}</div></div></body></html>";

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl);

        Assert.Equal(-1, ranking);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ThrowsExceptionWhenNoNodesMeetThreshold()
    {
        string htmlResponse = "<div><h3>Test</h3></div>"; // Insufficient nodes
        string targetUrl = "www.example.com";

        var exception = Assert.Throws<Exception>(() => _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl));
        Assert.Contains("The number of search result nodes is less than the expected amount", exception.Message);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ThrowsExceptionOnInvalidHtml()
    {
        string htmlResponse = "<div><h3>Test";
        string targetUrl = "www.example.com";

        var exception = Assert.Throws<Exception>(() => _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl));
        Assert.Contains("Error parsing HTML string from the response", exception.Message);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ThrowsExceptionWhenTooFewResults()
    {
        string targetUrl = "www.example.com";
        string htmlResponse = $"<body>{GenerateHtml(99, "only99.com")}</body>";

        var exception = Assert.Throws<Exception>(() => _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl));
        Assert.Contains("The number of search result nodes is less than the expected amount of: 100", exception.Message);
    }

    [Fact]
    public void ParseHtmlAndGetRanking_ReturnsNegativeOneWhenUrlNotFound()
    {
        string htmlResponse = $"<body>{GenerateHtml(105, "another-site.com")}</body>";
        string targetUrl = "www.example.com";

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl);

        Assert.Equal(-1, ranking);
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

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl);

        Assert.Equal(1, ranking);
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

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl);

        Assert.Equal(1, ranking);
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

        int ranking = _parserService.ParseHtmlAndGetRanking(htmlResponse, targetUrl);

        Assert.Equal(3, ranking);
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
