using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class BrandToolsPage : Page
{
    public BrandToolsViewModel ViewModel { get; }

    public BrandToolsPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<BrandToolsViewModel>();
    }
}
