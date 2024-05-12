using System.Windows.Controls;

namespace Smokeball.RankingAnalyser.WpfApp.Contracts.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();
}
