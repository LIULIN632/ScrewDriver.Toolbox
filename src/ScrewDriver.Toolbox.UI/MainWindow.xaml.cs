using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Services;
using ScrewDriver.Toolbox.UI.Views;
using Application = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI;

public partial class MainWindow : Window
{
    private TrayIconManager? _trayIconManager;
    private bool _categoryExpanded;
    private string _activeNavTag = "StartPage";
    private string? _activeCategory;

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

        // 默认高亮启动
        SetActiveNav("StartPage");
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
            SetActiveNav(tag);
            NavigateToPageByTag(tag);
        }
        else if (sender is Grid grid && grid.Tag is string gridTag)
        {
            SetActiveNav(gridTag);
            NavigateToPageByTag(gridTag);
        }
    }

    private void ToggleCategoryExpand(object sender, MouseButtonEventArgs e)
    {
        _categoryExpanded = !_categoryExpanded;
        CategoryPanel.Visibility = _categoryExpanded ? Visibility.Visible : Visibility.Collapsed;
        ExpandIcon.Text = _categoryExpanded ? "▼" : "▶";
        SetActiveNav("ToolRepositoryPage");

        // 展开时恢复之前选中的分类高亮
        if (_categoryExpanded && _activeCategory != null)
        {
            foreach (var child in CategoryPanel.Children)
            {
                if (child is Border cb && cb.DataContext is string cat && cat == _activeCategory)
                {
                    cb.Background = FindResource("PrimaryLightBrush") as System.Windows.Media.Brush;
                    if (cb.Child is TextBlock tb)
                        tb.Foreground = FindResource("PrimaryBrush") as System.Windows.Media.Brush;
                }
            }
        }

        e.Handled = true;
    }

    private void CategoryItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is string category)
        {
            // 清除之前分类的高亮
            foreach (var child in CategoryPanel.Children)
            {
                if (child is Border cb)
                {
                    cb.Background = System.Windows.Media.Brushes.Transparent;
                    if (cb.Child is TextBlock ct)
                        ct.Foreground = FindResource("TextSecondaryBrush") as System.Windows.Media.Brush;
                }
            }
            // 高亮当前分类
            _activeCategory = category;
            border.Background = FindResource("PrimaryLightBrush") as System.Windows.Media.Brush;
            if (border.Child is TextBlock tb)
                tb.Foreground = FindResource("PrimaryBrush") as System.Windows.Media.Brush;

            SetActiveNav("ToolRepositoryPage");
            MainFrame.Navigate(new ToolRepositoryPage(category));
        }
    }

    private void SetActiveNav(string tag)
    {
        _activeNavTag = tag;
        var navTags = new[] { "StartPage", "ToolRepositoryPage", "SystemOptimizerPage",
                              "RepairCenterPage", "HardwarePage", "ScenariosPage", "SettingsPage" };

        // 遍历导航栏所有子元素，清除/设置高亮
        if (NavContainer is StackPanel panel)
        {
            foreach (var child in panel.Children)
            {
                string? itemTag = child switch
                {
                    Border b => b.Tag as string,
                    Grid g => g.Tag as string,
                    _ => null
                };

                if (itemTag == null || !navTags.Contains(itemTag)) continue;

                bool isActive = itemTag == tag;
                var bg = isActive
                    ? FindResource("PrimaryLightBrush") as System.Windows.Media.Brush
                    : System.Windows.Media.Brushes.Transparent;
                var borderThickness = isActive ? new Thickness(4, 0, 0, 0) : new Thickness(0);

                switch (child)
                {
                    case Border b:
                        b.Background = bg;
                        b.BorderBrush = isActive ? FindResource("PrimaryBrush") as System.Windows.Media.Brush : null;
                        b.BorderThickness = borderThickness;
                        break;
                    case Grid g:
                        g.Background = bg;
                        // Grid doesn't have BorderBrush/Thickness, apply to inner Border
                        foreach (var inner in g.Children)
                        {
                            if (inner is Border ib)
                            {
                                ib.Background = bg;
                                ib.BorderBrush = isActive ? FindResource("PrimaryBrush") as System.Windows.Media.Brush : null;
                                ib.BorderThickness = borderThickness;
                            }
                        }
                        break;
                }
            }
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
