namespace Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;

public interface ISearchRequestService
{
    Task<string> SendSearchRequest(string keywords);
}
