using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Converters;
using ScrewDriver.Toolbox.UI;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class SettingsPage : System.Windows.Controls.UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void LightTheme_Click(object sender, RoutedEventArgs e) { ThemeService.SetTheme(AppTheme.Light); HighlightActiveTheme(); }
    private void DarkTheme_Click(object sender, RoutedEventArgs e) { ThemeService.SetTheme(AppTheme.Dark); HighlightActiveTheme(); }
    private void SystemTheme_Click(object sender, RoutedEventArgs e) { ThemeService.SetTheme(AppTheme.System); HighlightActiveTheme(); }

    private void HighlightActiveTheme()
    {
        var active = ThemeService.CurrentTheme;
        var activeBrush = FindResource("PrimaryBrush") as System.Windows.Media.Brush;
        var defaultBrush = FindResource("BorderBrush") as System.Windows.Media.Brush;

        foreach (var btn in new[] { LightThemeBtn, DarkThemeBtn, SystemThemeBtn })
        {
            btn.BorderBrush = defaultBrush;
            btn.BorderThickness = new Thickness(1);
            btn.FontWeight = FontWeights.Normal;
        }

        var activeBtn = active switch
        {
            AppTheme.Light => LightThemeBtn,
            AppTheme.Dark => DarkThemeBtn,
            AppTheme.System => SystemThemeBtn,
            _ => null
        };

        if (activeBtn != null)
        {
            activeBtn.BorderBrush = activeBrush;
            activeBtn.BorderThickness = new Thickness(2);
            activeBtn.FontWeight = FontWeights.SemiBold;
        }
    }

    private void GitHubLink_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("https://github.com/LIULIN632/ScrewDriver.Toolbox") { UseShellExecute = true }); }
        catch { }
    }

    private void AutoStart_Changed(object sender, RoutedEventArgs e)
    {
        try
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                       ?? System.AppContext.BaseDirectory + "ScrewDriver.Toolbox.exe";

            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;

            if (AutoStartCheck.IsChecked == true)
                key.SetValue("ScrewDriverToolbox", $"\"{exePath}\"");
            else
                key.DeleteValue("ScrewDriverToolbox", false);
        }
        catch { }
    }

    private readonly Core.Services.JsonConfigManager _settingsConfig = new(AppDomain.CurrentDomain.BaseDirectory);

    private void MinimizeToTray_Changed(object sender, RoutedEventArgs e)
    {
        try
        {
            _settingsConfig.Save("tray-mode", new TrayModeModel { MinimizeToTray = MinimizeToTrayCheck.IsChecked == true });
        }
        catch { }
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        HighlightActiveTheme();
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            if (key?.GetValue("ScrewDriverToolbox") != null)
                AutoStartCheck.IsChecked = true;
        }
        catch { }

        try
        {
            var trayMode = _settingsConfig.Load<TrayModeModel>("tray-mode");
            if (trayMode != null)
                MinimizeToTrayCheck.IsChecked = trayMode.MinimizeToTray;
        }
        catch { }
    }

    private void RescanIcons_Click(object sender, RoutedEventArgs e)
    {
        IconHelper.ClearCache();
        System.Windows.MessageBox.Show("图标缓存已清除，重新打开启动页后将自动提取最新图标。", "提示",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        BtnCheckUpdate.IsEnabled = false;
        TxtUpdateStatus.Text = "正在检查...";
        TxtUpdateStatus.Foreground = FindResource("TextSecondaryBrush") as System.Windows.Media.Brush;

        try
        {
            var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
            var latest = await UpdateChecker.CheckAppVersionAsync(currentVersion);

            if (latest != null)
            {
                TxtUpdateStatus.Text = $"发现新版本 v{latest}，当前 v{currentVersion}";
                TxtUpdateStatus.Foreground = FindResource("PrimaryBrush") as System.Windows.Media.Brush;
                BtnCheckUpdate.Content = "前往下载";

                var result = System.Windows.MessageBox.Show(
                    $"发现新版本 v{latest}\n\n当前版本：v{currentVersion}\n\n是否打开下载页面？",
                    "发现更新", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    try { Process.Start(new ProcessStartInfo("https://github.com/LIULIN632/ScrewDriver.Toolbox/releases/latest") { UseShellExecute = true }); }
                    catch { }
                }
            }
            else
            {
                TxtUpdateStatus.Text = "已是最新版本 ✓";
                TxtUpdateStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#22C55E"));
            }
        }
        catch
        {
            TxtUpdateStatus.Text = "检查失败，请检查网络连接";
            TxtUpdateStatus.Foreground = FindResource("ErrorBrush") as System.Windows.Media.Brush;
        }

        BtnCheckUpdate.IsEnabled = true;
    }

    private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
    {
        var keepTools = System.Windows.MessageBox.Show(
            "将重置主题、隐藏工具列表、页面状态等所有设置。\n\n是否保留已安装工具列表？",
            "恢复默认设置", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        if (keepTools == MessageBoxResult.Cancel) return;

        var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        var config = new JsonConfigManager(baseDir);

        // 删除所有配置文件
        foreach (var name in new[] { "window-state", "last-page", "category-state", "theme", "hidden-tools" })
            config.Delete(name);

        if (keepTools != MessageBoxResult.Yes)
        {
            config.Delete("pinned_tools");
            var localConfigDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ScrewDriverToolbox");
            if (System.IO.Directory.Exists(localConfigDir))
            {
                try { System.IO.Directory.Delete(localConfigDir, true); } catch { }
            }
        }

        ThemeService.SetTheme(AppTheme.Light);
        HighlightActiveTheme();
        System.Windows.MessageBox.Show("设置已恢复为默认值。", "完成",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
