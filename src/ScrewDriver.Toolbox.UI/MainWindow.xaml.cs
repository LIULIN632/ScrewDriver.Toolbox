using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Services;
using ScrewDriver.Toolbox.UI.ViewModels;
using ScrewDriver.Toolbox.UI.Views;
using Application = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI;

public partial class MainWindow : Window
{
    private TrayIconManager? _trayIconManager;
    private bool _categoryExpanded;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Navigate(new StartPage());

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

        // 预创建分类列表项（避免展开时卡顿）
        foreach (var cat in ToolRegistry.Categories)
        {
            var border = new Border
            {
                Padding = new Thickness(32, 0, 0, 0),
                Height = 36,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = System.Windows.Media.Brushes.Transparent,
                DataContext = cat
            };
            border.MouseLeftButtonDown += CategoryItem_Click;
            border.Child = new TextBlock
            {
                Text = cat,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = FindResource("TextSecondaryBrush") as System.Windows.Media.Brush
            };
            CategoryPanel.Children.Add(border);
        }
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

    private void NavItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is string tag)
        {
            NavigateToPageByTag(tag);
        }
    }

    private void ToggleCategoryExpand(object sender, MouseButtonEventArgs e)
    {
        _categoryExpanded = !_categoryExpanded;
        CategoryPanel.Visibility = _categoryExpanded ? Visibility.Visible : Visibility.Collapsed;
        ExpandIcon.Text = _categoryExpanded ? "▼" : "▶";
        e.Handled = true;
    }

    private void CategoryItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is string category)
        {
            MainFrame.Navigate(new ToolRepositoryPage(category));
        }
    }

    public void NavigateToPageByTag(string tag)
    {
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
