using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using Button = System.Windows.Controls.Button;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace ScrewDriver.Toolbox.UI.Views.Controls;

public partial class ToolCard : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty LaunchCommandProperty =
        DependencyProperty.Register(nameof(LaunchCommand), typeof(ICommand), typeof(ToolCard));

    public static readonly DependencyProperty InstallCommandProperty =
        DependencyProperty.Register(nameof(InstallCommand), typeof(ICommand), typeof(ToolCard));

    public static readonly DependencyProperty UpgradeCommandProperty =
        DependencyProperty.Register(nameof(UpgradeCommand), typeof(ICommand), typeof(ToolCard));

    public ToolCard()
    {
        InitializeComponent();
    }

    public ICommand? LaunchCommand
    {
        get => (ICommand?)GetValue(LaunchCommandProperty);
        set => SetValue(LaunchCommandProperty, value);
    }

    public ICommand? InstallCommand
    {
        get => (ICommand?)GetValue(InstallCommandProperty);
        set => SetValue(InstallCommandProperty, value);
    }

    public ICommand? UpgradeCommand
    {
        get => (ICommand?)GetValue(UpgradeCommandProperty);
        set => SetValue(UpgradeCommandProperty, value);
    }

    private void OpenUrl_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ToolItem tool
            && !string.IsNullOrEmpty(tool.OfficialUrl))
        {
            try { Process.Start(new ProcessStartInfo(tool.OfficialUrl) { UseShellExecute = true }); }
            catch { }
        }
    }

    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.DataContext is not ToolItem tool) return;

        var menu = new ContextMenu();

        if (!string.IsNullOrEmpty(tool.OfficialUrl))
        {
            var item = new MenuItem { Header = "官网" };
            item.Click += (_, _) =>
            {
                try { Process.Start(new ProcessStartInfo(tool.OfficialUrl) { UseShellExecute = true }); }
                catch { }
            };
            menu.Items.Add(item);
        }

        if (!string.IsNullOrEmpty(tool.GithubUrl))
        {
            var item = new MenuItem { Header = "GitHub" };
            item.Click += (_, _) =>
            {
                try { Process.Start(new ProcessStartInfo(tool.GithubUrl) { UseShellExecute = true }); }
                catch { }
            };
            menu.Items.Add(item);
        }

        if (!string.IsNullOrEmpty(tool.WingetId))
        {
            menu.Items.Add(new MenuItem { Header = $"winget: {tool.WingetId}", IsEnabled = false });
        }

        if (menu.Items.Count > 0)
        {
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }
}
