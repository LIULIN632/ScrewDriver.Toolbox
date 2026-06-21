using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScrewDriver.Toolbox.UI.ViewModels;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class SoftwareOptimizePage : System.Windows.Controls.UserControl
{
    private SoftwareOptimizeViewModel? _vm;

    public SoftwareOptimizePage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnPageUnloaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm != null)
            _vm.PropertyChanged -= OnViewModelPropertyChanged;

        _vm = DataContext as SoftwareOptimizeViewModel;
        if (_vm != null)
            _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SoftwareOptimizeViewModel.SelectedSoftware))
            Dispatcher.Invoke(() => HighlightChip(_vm!.SelectedSoftware));
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm = null;
        }
    }

    private void Chip_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is string software)
        {
            if (DataContext is SoftwareOptimizeViewModel vm)
                vm.SelectedSoftware = software;
        }
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
