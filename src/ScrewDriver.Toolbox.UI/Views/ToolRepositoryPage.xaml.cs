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
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class ToolRepositoryPage : Page
{
    public ToolRepositoryPage()
    {
        InitializeComponent();
        Loaded += Page_Loaded;
    }

    public ToolRepositoryPage(string initialCategory)
    {
        InitializeComponent();
        if (DataContext is ToolRepositoryViewModel vm)
            vm.SelectedCategory = initialCategory;
        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ToolRepositoryViewModel vm)
            vm.StartDetection();
    }

    private void Page_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
    }

    private void Page_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files == null) return;

        var vm = DataContext as ToolRepositoryViewModel;
        if (vm == null) return;

        foreach (var file in files)
        {
            string? targetPath = file;

            if (file.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                targetPath = ShortcutResolver.ResolveShortcut(file);
            else if (!file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath))
                continue;

            vm.AddDroppedTool(targetPath);
        }
    }

    private void RecommendedTool_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border) return;
        if (border.DataContext is not ToolItem tool) return;
        if (DataContext is not ToolRepositoryViewModel vm) return;

        if (tool.IsInstalled && !string.IsNullOrEmpty(tool.LaunchPath))
            vm.LaunchToolCommand.Execute(tool);
        else
            vm.InstallToolCommand.Execute(tool);
    }

    private void OpenUrl_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string url && !string.IsNullOrEmpty(url))
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { MessageBox.Show("无法打开链接", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }

    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not ToolItem tool) return;

        var menu = new ContextMenu();

        if (!string.IsNullOrEmpty(tool.OfficialUrl))
        {
            var officialItem = new MenuItem { Header = "官网" };
            officialItem.Click += (_, _) =>
            {
                try { Process.Start(new ProcessStartInfo(tool.OfficialUrl) { UseShellExecute = true }); }
                catch { }
            };
            menu.Items.Add(officialItem);
        }

        if (!string.IsNullOrEmpty(tool.GithubUrl))
        {
            var githubItem = new MenuItem { Header = "GitHub" };
            githubItem.Click += (_, _) =>
            {
                try { Process.Start(new ProcessStartInfo(tool.GithubUrl) { UseShellExecute = true }); }
                catch { }
            };
            menu.Items.Add(githubItem);
        }

        if (!string.IsNullOrEmpty(tool.WingetId))
        {
            var wingetItem = new MenuItem { Header = $"winget: {tool.WingetId}" };
            wingetItem.IsEnabled = false;
            menu.Items.Add(wingetItem);
        }

        if (menu.Items.Count > 0)
        {
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }
}
