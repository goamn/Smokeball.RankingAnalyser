using Smokeball.RankingAnalyser.WpfApp.Models;

namespace Smokeball.RankingAnalyser.WpfApp.Contracts.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme);

    AppTheme GetCurrentTheme();
}
