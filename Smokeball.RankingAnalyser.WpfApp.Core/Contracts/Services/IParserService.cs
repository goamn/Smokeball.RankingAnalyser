namespace Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;

public interface IParserService
{
    List<int> ParseHtmlAndGetRankings(string htmlResponse, string targetUrl);
}
