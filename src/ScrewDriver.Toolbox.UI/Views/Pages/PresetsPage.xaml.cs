using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.UI.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class PresetsPage : System.Windows.Controls.UserControl
{
    public PresetsPage()
    {
        InitializeComponent();
    }

    private void ResetDefaults_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PresetsViewModel vm)
        {
            var result = MessageBox.Show("将删除所有自定义预设并恢复为默认方案，确定继续？",
                "恢复默认", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
                vm.ResetToDefaults();
        }
    }
}
