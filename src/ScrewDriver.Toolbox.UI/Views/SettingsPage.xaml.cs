using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Converters;

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
        try { Process.Start(new ProcessStartInfo("https://github.com/LIULIN632/ScrewDriver.Toolbox") { UseShellExecute = true }); }
        catch { /* setting error */ }
    }

    private void RescanIcons_Click(object sender, RoutedEventArgs e)
    {
        IconHelper.ClearCache();
        System.Windows.MessageBox.Show("图标缓存已清除，重新打开启动页后将自动提取最新图标。", "提示",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
