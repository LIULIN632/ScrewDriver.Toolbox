using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class RepairCenterPage : Page
{
    public RepairCenterViewModel ViewModel { get; }

    public RepairCenterPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<RepairCenterViewModel>();
    }
}
