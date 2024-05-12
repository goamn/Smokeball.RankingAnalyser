using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Smokeball.RankingAnalyser.WpfApp.Contracts.Services;
using Smokeball.RankingAnalyser.WpfApp.Core.Helpers;
using Smokeball.RankingAnalyser.WpfApp.Models;
using Smokeball.RankingAnalyser.WpfApp.Services;
using Smokeball.RankingAnalyser.WpfApp.ViewModels;
using Smokeball.RankingAnalyser.WpfApp.Views;
using System.Reflection;
using Xunit;

namespace Smokeball.RankingAnalyser.WpfApp.Tests.xUnit;
public class MainPageTests
{
    private readonly IHost _host;
    public MainPageTests()
    {
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => c.SetBasePath(appLocation))
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Mocked Services
        var mockSeoService = new Mock<ISeoService>();
        services.AddSingleton(mockSeoService.Object);

        // Services
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Configuration
        services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
    }

    [Fact]
    public void MainViewModel_IsCreated()
    {
        var vm = _host.Services.GetService<MainViewModel>();
        Assert.NotNull(vm);
    }

    [Fact]
    public void MainViewModel_SeoServiceIsInjected()
    {
        var vm = _host.Services.GetService<MainViewModel>();
        Assert.NotNull(vm);
        var seoServiceField = typeof(MainViewModel).GetField("_seoService", BindingFlags.NonPublic | BindingFlags.Instance);
        var seoService = seoServiceField.GetValue(vm);
        Assert.NotNull(seoService);
        Assert.IsAssignableFrom<ISeoService>(seoService);
    }

    [Fact]
    public void MainViewModel_AnalyseCommandUpdatesValues()
    {
        var mockSeoService = new Mock<ISeoService>();
        mockSeoService.Setup(s => s.Analyse(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SearchRankingResult { Rankings = "1", RankingFound = true });

        var vm = new MainViewModel(mockSeoService.Object);
        Assert.Null(vm.Ranking?.ErrorMessage);
        Assert.Null(vm.Ranking?.Rankings);

        vm.AnalyseCommand.Execute(null);

        Assert.NotNull(vm.Ranking);
        Assert.True(vm.Ranking.RankingFound);
        Assert.Equal("1", vm.Ranking.Rankings);
        Assert.Null(vm.Ranking.ErrorMessage);
    }

    [Fact]
    public void MainViewModel_PropertyChangeInvoked()
    {
        var vm = _host.Services.GetService<MainViewModel>();
        var keywords = "new keywords";
        var propertyChangedInvoked = false;
        var newValue = "";
        vm.PropertyChanged += (values, e) =>
        {
            propertyChangedInvoked = e.PropertyName == "Keywords";
            newValue = ((MainViewModel)values).Keywords;
        };

        vm.Keywords = keywords;

        Assert.True(propertyChangedInvoked);
        Assert.Equal(keywords, newValue);
    }

    [Fact]
    public void MainViewModel_UIUpdatesWhenRankingFound()
    {
        var mockSeoService = new Mock<ISeoService>();
        var vm = new MainViewModel(mockSeoService.Object);
        var ranking = "3, 55";
        vm.Ranking = new SearchRankingResult { RankingFound = true, Rankings = ranking };

        Assert.Equal("Ranking found!", vm.ResultTitle);
        Assert.Equal($"The URL is ranked at position(s): {ranking}.", vm.ResultBody);
    }

    [Fact]
    public void MainViewModel_UIUpdatesWhenRankingNotFound()
    {
        var mockSeoService = new Mock<ISeoService>();
        var vm = new MainViewModel(mockSeoService.Object);
        var ranking = "0";
        vm.Ranking = new SearchRankingResult { RankingFound = false, Rankings = ranking };

        Assert.Equal("Ranking not found", vm.ResultTitle);
        Assert.Equal($"The URL is not in the top {Constants.SearchResultsCount} results.", vm.ResultBody);
    }

    [Fact]
    public void MainViewModel_UIUpdatesWhenErrorOccurs()
    {
        var mockSeoService = new Mock<ISeoService>();
        var vm = new MainViewModel(mockSeoService.Object);
        var errorMessage = "Service unavailable";
        vm.Ranking = new SearchRankingResult { ErrorMessage = errorMessage };

        Assert.Equal("There was an error", vm.ResultTitle);
        Assert.Equal(errorMessage, vm.ResultBody);
    }

    [Fact]
    public void MainPage_HasTypeOfMainViewModel()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            var pageType = pageService.GetPageType(typeof(MainViewModel).FullName);
            Assert.Equal(typeof(MainPage), pageType);
        }
        else
        {
            Assert.True(false, $"Can't resolve {nameof(IPageService)}");
        }
    }
}
