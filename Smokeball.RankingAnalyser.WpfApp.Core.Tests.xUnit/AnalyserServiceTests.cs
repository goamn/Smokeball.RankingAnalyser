using Moq;
using Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Services;
using System.Xml;
using Xunit;


namespace Smokeball.RankingAnalyser.WpfApp.Core.Tests.xUnit;

public class AnalyserServiceTests
{
    private readonly Mock<ISearchRequestService> _mockSearchRequestService;
    private readonly Mock<IParserService> _mockParserService;
    private readonly AnalyserService _analyserService;

    public AnalyserServiceTests()
    {
        _mockSearchRequestService = new Mock<ISearchRequestService>();
        _mockParserService = new Mock<IParserService>();
        _analyserService = new AnalyserService(_mockSearchRequestService.Object, _mockParserService.Object);
    }

    [Fact]
    public async Task Analyse_ReturnsRankingOnSuccess()
    {
        var keywords = "test";
        var targetUrl = "www.example.com";
        var html = "<html>Valid HTML content</html>";
        var expectedRanking = 5;

        _mockSearchRequestService.Setup(s => s.SendSearchRequest(keywords))
            .ReturnsAsync(html);
        _mockParserService.Setup(p => p.ParseHtmlAndGetRanking(html, targetUrl))
            .Returns(expectedRanking);

        var result = await _analyserService.Analyse(keywords, targetUrl);

        Assert.NotNull(result);
        Assert.Equal(expectedRanking, result.Result);
        Assert.Equal(default, result.TechnicalErrorDetails);
    }

    [Fact]
    public async Task Analyse_ReturnsErrorWhenSearchRequestFails()
    {
        var keywords = "test";
        var targetUrl = "www.example.com";
        var expectedErrorMessage = "Network error";

        _mockSearchRequestService.Setup(s => s.SendSearchRequest(keywords))
            .ThrowsAsync(new Exception(expectedErrorMessage));

        var result = await _analyserService.Analyse(keywords, targetUrl);

        Assert.Equal(default, result.Result);
        Assert.Equal(expectedErrorMessage, result.TechnicalErrorDetails);
    }

    [Fact]
    public async Task Analyse_ReturnsErrorWhenParsingFails()
    {
        var keywords = "test";
        var targetUrl = "www.example.com";
        var expectedHtml = "<html>Valid HTML content</html>";
        var expectedRrrorMessage = "Parsing error";

        _mockSearchRequestService.Setup(s => s.SendSearchRequest(keywords))
            .ReturnsAsync(expectedHtml);

        _mockParserService.Setup(p => p.ParseHtmlAndGetRanking(It.IsAny<string>(), targetUrl))
            .Throws(new XmlException(expectedRrrorMessage));

        var result = await _analyserService.Analyse(keywords, targetUrl);

        Assert.Equal(default, result.Result);
        Assert.Equal(expectedRrrorMessage, result.TechnicalErrorDetails);
    }

    [Fact]
    public async Task Analyse_CleansHtmlResponseBeforeParsing()
    {
        var keywords = "test";
        var targetUrl = "www.example.com";
        var reallyDirtyHtml = @$"<!doctype html><html><head><iwillnotclose>&amp;<script>maliciousContent();
function everyoneLovesJavascript(bool areYouSure) {{
  eval('eval(\'crash\')');
}}</script>&amp;<style>
@tailwind base;
/* Base theme overrides */
body {{
  @apply text-black dark:text-white;
}}
a {{
  @apply text-teams hover:text-hyperlink_hover dark:text-teams-light dark:hover:text-fluent-dark-teams_default_purple;
  text-decoration: none;
  font-weight: 600;
}}
</style>
{ParserServiceTests.GenerateHtml(50, "www.not-this-one.com")}
<div><h3>Real entry</h3><div>{targetUrl}</div></div>
{ParserServiceTests.GenerateHtml(50, "www.not-this-one.com")}</html>";

        var expectedCleanedHtml = @$"<html>
{ParserServiceTests.GenerateHtml(50, "www.not-this-one.com")}
<div><h3>Real entry</h3><div>{targetUrl}</div></div>
{ParserServiceTests.GenerateHtml(50, "www.not-this-one.com")}</html>";

        var expectedRanking = 55;

        _mockSearchRequestService.Setup(s => s.SendSearchRequest(keywords))
            .ReturnsAsync(reallyDirtyHtml);

        _mockParserService.Setup(p => p.ParseHtmlAndGetRanking(expectedCleanedHtml, targetUrl))
            .Returns(expectedRanking);

        var result = await _analyserService.Analyse(keywords, targetUrl);

        _mockParserService.Verify(p => p.ParseHtmlAndGetRanking(It.Is<string>(s => s == expectedCleanedHtml), targetUrl), Times.Once());
        Assert.Equal(expectedRanking, result.Result);
    }

    [Fact]
    public async Task Analyse_ParserDoesNotThrowErrorFromCleanedHtml()
    {
        var keywords = "test";
        var targetUrl = "www.example.com";
        var reallyDirtyHtml = @$"<!doctype html><html><head><iwillnotclose>&amp;<script>maliciousContent();
function everyoneLovesJavascript(bool areYouSure) {{
  eval('eval(\'crash\')');
}}</script>&amp;<style>
@tailwind base;
/* Base theme overrides */
body {{
  @apply text-black dark:text-white;
}}
a {{
  @apply text-teams hover:text-hyperlink_hover dark:text-teams-light dark:hover:text-fluent-dark-teams_default_purple;
  text-decoration: none;
  font-weight: 600;
}}
</style>
{ParserServiceTests.GenerateHtml(50, "www.not-this-one.com")}
<div><h3>Real entry</h3><div>{targetUrl}</div></div>
{ParserServiceTests.GenerateHtml(50, "www.not-this-one.com")}
<img href='www.example.com/?q=test'>href should be removed</img>
<form>all forms should be cleaned<ul><li selected>1</li></ul></form></html>";
        var expectedRanking = 51;
        var analyserServiceWithRealParser = new AnalyserService(_mockSearchRequestService.Object, new ParserService());

        _mockSearchRequestService.Setup(s => s.SendSearchRequest(keywords))
            .ReturnsAsync(reallyDirtyHtml);

        var result = await analyserServiceWithRealParser.Analyse(keywords, targetUrl);

        Assert.Equal(expectedRanking, result.Result);
    }
}
