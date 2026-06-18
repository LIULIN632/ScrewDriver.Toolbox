using System.Windows;
using System.Windows.Controls;
using WpfApp = System.Windows.Application;
using ScrewDriver.Toolbox.UI.ViewModels;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class OptimizeDetailPage : Page
{
    public OptimizeDetailPage()
    {
        InitializeComponent();
    }

    public OptimizeDetailPage(string categoryName) : this()
    {
        TxtTitle.Text = categoryName;
        var vm = new OptimizeDetailViewModel();
        vm.LoadSettingsByCategory(categoryName);
        DataContext = vm;
        SettingList.ItemsSource = vm.SettingItems;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService.CanGoBack)
            NavigationService.GoBack();
        else if (WpfApp.Current.MainWindow is MainWindow main)
            main.MainFrame.Navigate(new SystemOptimizerPage());
    }
}
