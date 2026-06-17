using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.ViewModels;
using Button = System.Windows.Controls.Button;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class StartPage : Page
{
    private ToolItem? _contextTool;

    public StartPage()
    {
        InitializeComponent();
    }

    private void ToolIcon_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.DataContext is not ToolItem tool) return;

        string? path = tool.LocalExePath ?? tool.LaunchPath;
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show($"工具文件缺失\n\n{tool.Name} 的程序文件未找到。",
                "启动失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ToolContextMenu_Opening(object sender, ContextMenuEventArgs e)
    {
        if (sender is Border border)
            _contextTool = border.DataContext as ToolItem;
    }

    private void MenuOpen_Click(object sender, RoutedEventArgs e)
    {
        if (_contextTool == null) return;
        string? path = _contextTool.LocalExePath ?? _contextTool.LaunchPath;
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
            catch { }
        }
    }

    private void MenuOpenLocation_Click(object sender, RoutedEventArgs e)
    {
        if (_contextTool == null) return;
        string? path = _contextTool.LocalExePath ?? _contextTool.LaunchPath;
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir))
                    Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            catch { }
        }
    }

    private void MenuRemove_Click(object sender, RoutedEventArgs e)
    {
        if (_contextTool == null) return;
        if (DataContext is StartPageViewModel vm)
            vm.RemoveToolCommand?.Execute(_contextTool.Name);
    }
}
