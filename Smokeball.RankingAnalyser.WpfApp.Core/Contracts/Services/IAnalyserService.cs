using Smokeball.RankingAnalyser.WpfApp.Core.Models;

namespace Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;

public interface IAnalyserService
{
    Task<AnalysisResult> Analyse(string keywords, string targetUrl);
}
