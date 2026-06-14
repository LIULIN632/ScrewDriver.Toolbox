using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class HardwareCenterPage : Page
{
    public HardwareCenterViewModel ViewModel { get; }

    public HardwareCenterPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<HardwareCenterViewModel>();
    }
}
