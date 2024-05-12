using System.Windows.Controls;

namespace Smokeball.RankingAnalyser.WpfApp.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
