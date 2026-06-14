using Microsoft.UI.Xaml.Controls;
using ScrewDriver.Toolbox.ViewModels;

namespace ScrewDriver.Toolbox.Views;

public sealed partial class ToolRepositoryPage : Page
{
    public ToolRepositoryViewModel ViewModel { get; }

    public ToolRepositoryPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<ToolRepositoryViewModel>();
        ViewModel.LoadToolsCommand.Execute(null);
    }
}
