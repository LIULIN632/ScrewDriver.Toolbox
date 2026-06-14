using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Services;
using ScrewDriver.Toolbox.UI.Views;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;

namespace ScrewDriver.Toolbox.UI;

public partial class MainWindow : Window
{
    private TrayIconManager? _trayIconManager;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new StartPage());
        NavListBox.SelectedIndex = 0;

        UpdateThemeButton();
        ThemeService.ThemeChanged += (_, _) =>
        {
            Dispatcher.Invoke(UpdateThemeButton);
        };

        SourceInitialized += (_, _) =>
        {
            _trayIconManager = new TrayIconManager(ShowWindow, ExitApplication);
            _trayIconManager.Initialize();
        };
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        _trayIconManager?.Dispose();
        Application.Current.Shutdown();
    }

    private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavListBox.SelectedItem is not ListBoxItem item) return;

        Page? page = item.Tag.ToString() switch
        {
            "StartPage" => new StartPage(),
            "ToolRepositoryPage" => new ToolRepositoryPage(),
            "SystemOptimizerPage" => new SystemOptimizerPage(),
            "RepairCenterPage" => new RepairCenterPage(),
            "HardwarePage" => new HardwarePage(),
            "ScenariosPage" => new ScenariosPage(),
            _ => null
        };

        if (page != null) MainFrame.Navigate(page);
    }

    public void NavigateToPageByTag(string tag)
    {
        foreach (ListBoxItem item in NavListBox.Items)
        {
            if (item.Tag is string s && s == tag)
            {
                item.IsSelected = true;
                return;
            }
        }

        Page? page = tag switch
        {
            "StartPage" => new StartPage(),
            "ToolRepositoryPage" => new ToolRepositoryPage(),
            "SystemOptimizerPage" => new SystemOptimizerPage(),
            "RepairCenterPage" => new RepairCenterPage(),
            "HardwarePage" => new HardwarePage(),
            "ScenariosPage" => new ScenariosPage(),
            _ => null
        };

        if (page != null) MainFrame.Navigate(page);
    }

    private void ThemeToggleBtn_Click(object sender, RoutedEventArgs e)
    {
        var next = ThemeService.CurrentTheme switch
        {
            AppTheme.Light => AppTheme.Dark,
            AppTheme.Dark => AppTheme.System,
            AppTheme.System => AppTheme.Light,
            _ => AppTheme.Light
        };
        ThemeService.SetTheme(next);
        UpdateThemeButton();
    }

    private void UpdateThemeButton()
    {
        ThemeToggleBtn.Content = ThemeService.CurrentTheme switch
        {
            AppTheme.Light => "☀",
            AppTheme.Dark => "🌙",
            AppTheme.System => "🖥",
            _ => "☀"
        };
    }
}
