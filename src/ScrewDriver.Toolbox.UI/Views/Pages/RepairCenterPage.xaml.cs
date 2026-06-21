using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.UI.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class RepairCenterPage : System.Windows.Controls.UserControl
{
    public RepairCenterPage()
    {
        InitializeComponent();
    }

    private void InstalledTool_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { DataContext: ToolItem tool }) return;

        string? path = tool.LocalExePath ?? tool.LaunchPath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            MessageBox.Show($"工具文件缺失\n\n{tool.Name} 的程序文件未找到。",
                "启动失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

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
}
