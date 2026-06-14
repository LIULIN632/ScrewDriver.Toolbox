using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<DashboardViewModel>();
        ViewModel.LoadDataCommand.Execute(null);
    }
}
