using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ScrewDriver.Toolbox.Views;
using System;

namespace ScrewDriver.Toolbox;

public sealed partial class AppShell : NavigationView
{
    public AppShell()
    {
        InitializeComponent();
        SelectionChanged += OnSelectionChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 默认导航到首页
        SelectedItem = MenuItems[0];
        NavigateToPage("dashboard");
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateToPage(tag);
        }
    }

    private void NavigateToPage(string tag)
    {
        Type? pageType = tag switch
        {
            "dashboard" => typeof(DashboardPage),
            "tools"     => typeof(ToolRepositoryPage),
            "optimize"  => typeof(SystemOptimizerPage),
            "repair"    => typeof(RepairCenterPage),
            "hardware"  => typeof(HardwareCenterPage),
            "brand"     => typeof(BrandToolsPage),
            "scenario"  => typeof(ScenarioPage),
            "data"      => typeof(DataCenterPage),
            "settings"  => typeof(SettingsPage),
            _           => typeof(DashboardPage)
        };

        ContentFrame.Navigate(pageType, null, new EntranceNavigationTransitionInfo());
    }
}
