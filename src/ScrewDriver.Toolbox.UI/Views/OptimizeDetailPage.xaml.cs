using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.UI.ViewModels;
using WpfApp = System.Windows.Application;

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
        var vm = new OptimizeDetailViewModel(categoryName);
        DataContext = vm;
        vm.LoadSettings();
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
