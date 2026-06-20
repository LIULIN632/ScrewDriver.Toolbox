using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.ViewModels;
using ScrewDriver.Toolbox.UI.Views.Controls;
using Button = System.Windows.Controls.Button;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class StartPage : System.Windows.Controls.UserControl
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

    // 内置工具
    private void BuiltInHosts_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var hosts = @"C:\Windows\System32\drivers\etc\hosts";
            if (System.IO.File.Exists(hosts))
                Process.Start(new ProcessStartInfo("notepad.exe", hosts) { UseShellExecute = true });
        }
        catch { }
    }

    private void BuiltInKms_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("cmd.exe", "/k slmgr.vbs -dlv") { UseShellExecute = true, Verb = "runas" }); }
        catch { }
    }

    private void BuiltInWifi_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("cmd.exe", "/k netsh wlan show profiles") { UseShellExecute = true }); }
        catch { }
    }

    private void BuiltInCert_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("certmgr.msc") { UseShellExecute = true }); }
        catch { }
    }

    private void BuiltInUninstall_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var w = new CleanWindow();
            w.Owner = Window.GetWindow(this);
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.ShowDialog();
        }
        catch { }
    }

    private void BuiltInClean_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var w = new CleanWindow();
            w.Owner = Window.GetWindow(this);
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.ShowDialog();
        }
        catch { }
    }
}
