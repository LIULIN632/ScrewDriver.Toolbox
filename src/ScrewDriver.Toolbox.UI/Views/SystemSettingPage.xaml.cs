using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrewDriver.Toolbox.UI.ViewModels;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class SystemSettingPage : Page
{
    public SystemSettingPage()
    {
        InitializeComponent();
    }

    private void Category_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is string category)
        {
            if (DataContext is SystemSettingViewModel vm)
                vm.SelectedCategory = category;
        }
    }
}
