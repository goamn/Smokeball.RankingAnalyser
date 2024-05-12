using Smokeball.RankingAnalyser.WpfApp.Models;

namespace Smokeball.RankingAnalyser.WpfApp.Contracts.Services;

public interface ISeoService
{
    Task<SearchRankingResult> Analyse(string keywords, string targetUrl);
}
