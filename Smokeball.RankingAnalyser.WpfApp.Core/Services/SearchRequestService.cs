using Smokeball.RankingAnalyser.WpfApp.Core.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Helpers;

namespace Smokeball.RankingAnalyser.WpfApp.Core.Services;

public class SearchRequestService(IHttpClientFactory httpClientFactory) : ISearchRequestService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<string> SendSearchRequest(string keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
        {
            throw new ArgumentException("Keywords cannot be null or empty", nameof(keywords));
        }

        var url = string.Format(Constants.GoogleSearchUrl, keywords, Constants.SearchResultsCount);
        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetStringAsync(url);
            return response;
        }
        catch (Exception exception)
        {
            throw new Exception($"Error sending GET request: {exception.Message}", exception);
        }
    }
}
