using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.UI.ViewModels;
using Button = System.Windows.Controls.Button;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class SystemSettingsProPage : System.Windows.Controls.UserControl
{
    private SystemSettingsProViewModel? _vm;

    public SystemSettingsProPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnPageUnloaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm != null)
            _vm.PropertyChanged -= OnViewModelPropertyChanged;

        _vm = DataContext as SystemSettingsProViewModel;
        if (_vm != null)
            _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemSettingsProViewModel.SelectedCategory))
            Dispatcher.Invoke(() => HighlightChip(_vm!.SelectedCategory));
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm = null;
        }
    }

    private void Category_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is string cat)
        {
            if (DataContext is SystemSettingsProViewModel vm)
                vm.SelectedCategory = cat;
        }
    }

    private void Preset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is PresetDefinition preset)
        {
            if (DataContext is SystemSettingsProViewModel vm)
                vm.ApplyPreset(preset);
        }
    }

    private void RestoreCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string cat)
        {
            if (DataContext is SystemSettingsProViewModel vm)
                vm.RestoreCategory(cat);
        }
    }

    private void RestoreAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SystemSettingsProViewModel vm)
            vm.RestoreAll();
    }

    private void HighlightChip(string active)
    {
        if (ChipTabs is not ItemsControl ic) return;
        foreach (var child in LogicalTreeHelper.GetChildren(ic))
        {
            if (child is Border cb && cb.DataContext is string tag)
            {
                var isActive = tag == active;
                cb.Background = isActive ? FindResource("PrimaryBrush") as Brush : Brushes.Transparent;
                cb.BorderBrush = isActive ? FindResource("PrimaryBrush") as Brush : FindResource("BorderBrush") as Brush;
                if (cb.Child is TextBlock tb)
                    tb.Foreground = isActive ? Brushes.White : FindResource("TextSecondaryBrush") as Brush;
            }
        }
    }
}
