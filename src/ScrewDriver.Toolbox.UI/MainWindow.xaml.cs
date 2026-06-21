using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Services;
using ScrewDriver.Toolbox.UI.ViewModels;
using Application = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI;

public partial class MainWindow : Window
{
    private TrayIconManager? _trayIconManager;
    private bool _isExiting;
    private WindowStateModel? _restoredState;
    private readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        _restoredState = _config.Load<WindowStateModel>("window-state");
        if (_restoredState != null)
        {
            if (_restoredState.Width > 0) Width = _restoredState.Width;
            if (_restoredState.Height > 0) Height = _restoredState.Height;
        }

        SourceInitialized += (_, _) =>
        {
            _trayIconManager = new TrayIconManager(ShowWindow, ExitApplication);
            _trayIconManager.Initialize();
            ApplyTitleBarTheme();

            if (_restoredState != null)
            {
                if (_restoredState.Left >= 0 && _restoredState.Top >= 0)
                {
                    Left = _restoredState.Left;
                    Top = _restoredState.Top;
                }
                if (Enum.TryParse<WindowState>(_restoredState.State, out var ws))
                    WindowState = ws;
            }
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
        SaveWindowState();

        if (!_isExiting)
        {
            var trayMode = _config.Load<TrayModeModel>("tray-mode");
            if (trayMode?.MinimizeToTray == true)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            _isExiting = true;
            _trayIconManager?.Dispose();
            Application.Current.Shutdown();
            return;
        }

        _trayIconManager?.Dispose();
        base.OnClosing(e);
    }

    private void SaveWindowState()
    {
        var state = new WindowStateModel
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            State = WindowState.ToString()
        };
        _config.Save("window-state", state);
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

    private void NavItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (fe.DataContext is not NavigationItem item) return;
        if (fe.Tag is not string tag) return;

        var vm = (MainViewModel)DataContext;
        e.Handled = true;

        if (item.SubItems.Count > 0)
        {
            if (item.IsActive)
                item.IsExpanded = !item.IsExpanded;
            else
            {
                item.IsExpanded = true;
                vm.NavigateTo(tag);
            }
        }
        else
        {
            vm.NavigateTo(tag);
        }
    }
}

internal class TrayModeModel { public bool MinimizeToTray { get; set; } }
