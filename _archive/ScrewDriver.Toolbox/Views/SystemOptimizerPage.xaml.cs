using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class SystemOptimizerPage : Page
{
    public SystemOptimizerViewModel ViewModel { get; }

    public SystemOptimizerPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<SystemOptimizerViewModel>();
        ViewModel.LoadSettingsCommand.Execute(null);
    }
}
