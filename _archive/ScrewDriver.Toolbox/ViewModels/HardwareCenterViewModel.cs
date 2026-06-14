using CommunityToolkit.Mvvm.ComponentModel;
using ScrewDriver.Toolbox.Models;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class HardwareCenterViewModel : BaseViewModel
{
    [ObservableProperty]
    private DeviceModel _deviceInfo = new();

    [ObservableProperty]
    private ObservableCollection<HardwareStatus> _componentStatuses = [];

    [ObservableProperty]
    private string _selectedComponent = "CPU";

    public HardwareCenterViewModel()
    {
        PageTitle = "硬件检测";
        PageDescription = "查看硬件信息与实时状态（开发中）";
    }
}
