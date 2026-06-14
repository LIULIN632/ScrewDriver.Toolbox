using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Hardware.Services;
using ScrewDriver.Toolbox.UI.Views;
using WpfApp = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class HardwareDetailViewModel : BaseViewModel
{
    private readonly HardwareService _service = new();
    private HardwareDetailModule? _selectedModule;

    public ObservableCollection<HardwareDetailModule> Modules { get; } = new();

    public HardwareDetailModule? SelectedModule
    {
        get => _selectedModule;
        set
        {
            if (SetProperty(ref _selectedModule, value))
            {
                OnPropertyChanged(nameof(SelectedModule));
                OnPropertyChanged(nameof(SelectedModuleItems));
            }
        }
    }

    public List<HardwareDetailItem>? SelectedModuleItems => SelectedModule?.Items;

    public ICommand GoBackCommand { get; }

    public HardwareDetailViewModel()
    {
        GoBackCommand = new RelayCommand(_ => GoBack());

        var all = _service.GetAllDetailInfo();
        foreach (var m in all)
            Modules.Add(m);

        if (Modules.Count > 0)
            SelectedModule = Modules[0];
    }

    private static void GoBack()
    {
        if (WpfApp.Current.MainWindow is MainWindow main)
        {
            main.MainFrame.Navigate(new HardwarePage());
        }
    }
}
