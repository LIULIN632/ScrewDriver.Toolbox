using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.Core.Services;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void LightTheme_Click(object sender, RoutedEventArgs e) => ThemeService.SetTheme(AppTheme.Light);
    private void DarkTheme_Click(object sender, RoutedEventArgs e) => ThemeService.SetTheme(AppTheme.Dark);
    private void SystemTheme_Click(object sender, RoutedEventArgs e) => ThemeService.SetTheme(AppTheme.System);

    private void GitHubLink_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("https://github.com") { UseShellExecute = true }); }
        catch { }
    }
}
