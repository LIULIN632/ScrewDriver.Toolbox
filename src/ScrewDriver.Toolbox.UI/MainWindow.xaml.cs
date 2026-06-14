using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Services;
using ScrewDriver.Toolbox.UI.Views;
using Application = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI;

public partial class MainWindow : Window
{
    private TrayIconManager? _trayIconManager;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new StartPage());
        NavListBox.SelectedIndex = 0;

        SourceInitialized += (_, _) =>
        {
            _trayIconManager = new TrayIconManager(ShowWindow, ExitApplication);
            _trayIconManager.Initialize();
            ApplyTitleBarTheme();
        };

        ThemeService.ThemeChanged += (_, _) =>
        {
            Dispatcher.Invoke(ApplyTitleBarTheme);
        };
    }

    private void ApplyTitleBarTheme()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;
        var useDark = ThemeService.IsDarkMode() ? 1 : 0;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // 直接退出进程，释放文件锁，方便下次打包
        _trayIconManager?.Dispose();
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
            "SettingsPage" => new SettingsPage(),
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
            "SettingsPage" => new SettingsPage(),
            _ => null
        };

        if (page != null) MainFrame.Navigate(page);
    }
}
