using System.Windows.Controls;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.UI.ViewModels;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class DataCenterPage : System.Windows.Controls.UserControl
{
    public DataCenterPage()
    {
        InitializeComponent();
    }

    private void BackupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Selection is handled by binding to SelectedBackup
    }
}
