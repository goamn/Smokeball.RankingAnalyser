using Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Models;
using System.Text.RegularExpressions;

namespace Smokeball.RankingAnalyser.WpfApp.Core.Services;

public class AnalyserService(ISearchRequestService _searchRequestService, IParserService _parserService) : IAnalyserService
{
    public async Task<AnalysisResult> Analyse(string keywords, string targetUrl)
    {
        string htmlResponse;
        try
        {
            htmlResponse = await _searchRequestService.SendSearchRequest(keywords);
        }
        catch (Exception exception)
        {
            return new AnalysisResult
            {
                TechnicalErrorDetails = exception.Message
            };
        }

        htmlResponse = CleanResponse(htmlResponse);

        try
        {
            var rankings = _parserService.ParseHtmlAndGetRankings(htmlResponse, targetUrl);
            return new AnalysisResult
            {
                Results = rankings
            };
        }
        catch (Exception exception)
        {
            return new AnalysisResult
            {
                TechnicalErrorDetails = exception.Message
            };
        }
    }

    private string CleanResponse(string htmlResponse)
    {
        // Remove doctype, script tags, style tags and HTML entities (i.e. &amp;)
        htmlResponse = Regex.Replace(htmlResponse, "^<.*?doctype.*?html>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        htmlResponse = Regex.Replace(htmlResponse, "<script.*?script>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        htmlResponse = Regex.Replace(htmlResponse, "<style.*?style>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        htmlResponse = Regex.Replace(htmlResponse, "<form.*?form>", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        htmlResponse = Regex.Replace(htmlResponse, "&[a-zA-Z0-9#]{1,10};", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        htmlResponse = Regex.Replace(htmlResponse, "href=\"[^\"]*\"", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        // Remove non self-closing tags
        htmlResponse = Regex.Replace(htmlResponse, "<(\\w+)(?:\\s+[^>]*)?>(?!.*<\\/\\1>)", string.Empty, RegexOptions.Singleline);
        return htmlResponse;
    }
}
