namespace Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;

public interface IParserService
{
    int ParseHtmlAndGetRanking(string htmlResponse, string targetUrl);
}
