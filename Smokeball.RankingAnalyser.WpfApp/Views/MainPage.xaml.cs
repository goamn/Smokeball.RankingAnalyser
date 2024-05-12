using System.Windows.Controls;

using Smokeball.RankingAnalyser.WpfApp.ViewModels;

namespace Smokeball.RankingAnalyser.WpfApp.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
