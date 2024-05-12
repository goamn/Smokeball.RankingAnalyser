using Smokeball.RankingAnalyser.WpfApp.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Models;

namespace Smokeball.RankingAnalyser.WpfApp.Services;

public class SeoService(IAnalyserService _analyserService) : ISeoService
{

    public async Task<SearchRankingResult> Analyse(string keywords, string targetUrl)
    {
        var errorMessageTemplate = @"The search ranking could not be completed, please contact the Dev responsible for this application!

Technical information: {0}";

        var analysisResult = await _analyserService.Analyse(keywords, targetUrl);

        var isTechnicalError = !string.IsNullOrEmpty(analysisResult.TechnicalErrorDetails);
        var rankingFound = analysisResult.Results?.FirstOrDefault() != -1;
        return new SearchRankingResult
        {
            RankingFound = rankingFound,
            Rankings = rankingFound ? string.Join(", ", analysisResult.Results) : null,
            ErrorMessage = isTechnicalError ? string.Format(errorMessageTemplate, analysisResult.TechnicalErrorDetails) : null
        };
    }
}
