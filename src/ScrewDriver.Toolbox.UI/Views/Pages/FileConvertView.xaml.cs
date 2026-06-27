using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.UI.ViewModels;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class FileConvertView : System.Windows.Controls.UserControl
{
    public FileConvertView()
    {
        InitializeComponent();
    }

    private void UserControl_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            var files = (string[]?)e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (files != null && DataContext is FileConvertViewModel vm)
            {
                vm.AddFiles(files);
            }
        }
    }
}
