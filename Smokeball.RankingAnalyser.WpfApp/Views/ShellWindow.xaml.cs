using System.Windows.Controls;

using MahApps.Metro.Controls;

using Smokeball.RankingAnalyser.WpfApp.Contracts.Views;
using Smokeball.RankingAnalyser.WpfApp.ViewModels;

namespace Smokeball.RankingAnalyser.WpfApp.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();
}
