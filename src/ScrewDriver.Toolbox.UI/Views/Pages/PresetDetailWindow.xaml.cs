using System.Windows;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class PresetDetailWindow : Window
{
    private readonly PresetItem _preset;

    public PresetDetailWindow(PresetItem preset)
    {
        InitializeComponent();
        _preset = preset;
        DataContext = preset;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {
        _preset.ApplyCommand?.Execute(null);
        Close();
    }
}
