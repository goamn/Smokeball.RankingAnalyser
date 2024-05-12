namespace Smokeball.RankingAnalyser.WpfApp.Models;

public class SearchRankingResult
{
    public bool RankingFound { get; set; } = false;

    public string Rankings { get; set; }

    public string ErrorMessage { get; set; }
}
