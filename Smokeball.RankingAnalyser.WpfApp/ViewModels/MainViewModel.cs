using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smokeball.RankingAnalyser.WpfApp.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Helpers;
using Smokeball.RankingAnalyser.WpfApp.Models;
using System.Windows.Input;

namespace Smokeball.RankingAnalyser.WpfApp.ViewModels;

public class MainViewModel(ISeoService seoService) : ObservableObject
{
    private readonly ISeoService _seoService = seoService;
    private RelayCommand _analyseCommand;
    private string _keywords = "conveyancing software";
    private string _targetUrl = "www.smokeball.com.au";
    private SearchRankingResult _ranking;

    private bool _isError => _ranking?.ErrorMessage != null;

    public ICommand AnalyseCommand => _analyseCommand ??= new RelayCommand(async () => await OnAnalyse());

    public string Keywords
    {
        get => _keywords;
        set => SetProperty(ref _keywords, value);
    }

    public string TargetUrl
    {
        get => _targetUrl;
        set => SetProperty(ref _targetUrl, value);
    }
    public SearchRankingResult Ranking
    {
        get => _ranking;
        set
        {
            if (SetProperty(ref _ranking, value))
            {
                OnPropertyChanged(nameof(ResultTitle));
                OnPropertyChanged(nameof(ResultBody));
            }
        }
    }

    public string ResultTitle
    {
        get
        {
            return _isError switch
            {
                true => "There was an error",
                false when _ranking?.RankingFound == true => "Ranking found!",
                false when _ranking?.RankingFound == false => "Ranking not found",
                _ => string.Empty
            };
        }
    }

    public string ResultBody
    {
        get
        {
            return _isError switch
            {
                true => _ranking?.ErrorMessage,
                false when _ranking?.RankingFound == true => $"The URL is ranked at position(s): {_ranking?.Rankings}.",
                false when _ranking?.RankingFound == false => $"The URL is not in the top {Constants.SearchResultsCount} results.",
                _ => "Click the Analyse button to check URL ranking.",
            };
        }
    }


    private async Task OnAnalyse()
    {
        Ranking = await _seoService.Analyse(Keywords, TargetUrl);
    }
}
