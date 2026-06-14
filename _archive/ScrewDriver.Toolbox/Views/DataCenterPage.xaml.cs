using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class DataCenterPage : Page
{
    public DataCenterViewModel ViewModel { get; }

    public DataCenterPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<DataCenterViewModel>();
    }
}
