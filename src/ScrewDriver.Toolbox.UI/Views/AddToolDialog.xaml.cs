using System.IO;
using System.Windows;
using ScrewDriver.Toolbox.Core.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class AddToolDialog : Window
{
    public string ToolName { get; private set; }
    public string ToolPath { get; }
    public string SelectedCategory { get; private set; } = "效率工具";

    public AddToolDialog(string exePath, List<string> categories)
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;

        ToolPath = exePath;
        NameBox.Text = Path.GetFileNameWithoutExtension(exePath);
        ToolName = NameBox.Text;

        CategoryCombo.ItemsSource = categories;
        CategoryCombo.SelectedItem = "效率工具";

        NameBox.Focus();
        NameBox.SelectAll();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("请输入工具名称。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ToolName = name;
        SelectedCategory = (CategoryCombo.SelectedItem as string) ??
                           CategoryCombo.Text?.Trim() ?? "效率工具";
        DialogResult = true;
        Close();
    }
}
