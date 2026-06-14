using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class ScenarioPage : Page
{
    public ScenarioViewModel ViewModel { get; }

    public ScenarioPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<ScenarioViewModel>();
    }
}
