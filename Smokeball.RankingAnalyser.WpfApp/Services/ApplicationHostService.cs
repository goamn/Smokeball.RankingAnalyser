using Microsoft.Extensions.Hosting;

using Smokeball.RankingAnalyser.WpfApp.Contracts.Activation;
using Smokeball.RankingAnalyser.WpfApp.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Contracts.Views;
using Smokeball.RankingAnalyser.WpfApp.ViewModels;

namespace Smokeball.RankingAnalyser.WpfApp.Services;

public class ApplicationHostService(
    IServiceProvider serviceProvider,
    IEnumerable<IActivationHandler> activationHandlers,
    INavigationService navigationService,
    IThemeSelectorService themeSelectorService
    ) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly IEnumerable<IActivationHandler> _activationHandlers = activationHandlers;
    private IShellWindow _shellWindow;
    private bool _isInitialized;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Initialize services that you need before app activation
        await InitializeAsync();

        await HandleActivationAsync();

        // Tasks after activation
        await StartupAsync();
        _isInitialized = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _themeSelectorService.InitializeTheme();
            await Task.CompletedTask;
        }
    }

    private async Task StartupAsync()
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private async Task HandleActivationAsync()
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle());

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync();
        }

        await Task.CompletedTask;

        if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainViewModel).FullName);
            await Task.CompletedTask;
        }
    }
}
