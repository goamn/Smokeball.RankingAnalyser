using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smokeball.RankingAnalyser.WpfApp.Contracts.Services;
using System.Windows.Input;

namespace Smokeball.RankingAnalyser.WpfApp.ViewModels;

public class ShellViewModel(INavigationService navigationService) : ObservableObject
{
    private readonly INavigationService _navigationService = navigationService;
    private RelayCommand _goBackCommand;
    private RelayCommand _loadedCommand;
    private RelayCommand _unloadedCommand;

    public RelayCommand GoBackCommand => _goBackCommand ??= new RelayCommand(OnGoBack, CanGoBack);
    public ICommand LoadedCommand => _loadedCommand ??= new RelayCommand(OnLoaded);
    public ICommand UnloadedCommand => _unloadedCommand ??= new RelayCommand(OnUnloaded);

    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
    }

    private bool CanGoBack()
        => _navigationService.CanGoBack;

    private void OnGoBack()
        => _navigationService.GoBack();

    private void OnNavigated(object sender, string viewModelName)
        => GoBackCommand.NotifyCanExecuteChanged();
}
