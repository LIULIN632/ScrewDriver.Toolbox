using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScrewDriver.Toolbox.Models;
using ScrewDriver.Toolbox.Services;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IDeviceInfoService _deviceInfoService;

    [ObservableProperty]
    private DeviceModel _deviceInfo = new();

    [ObservableProperty]
    private HardwareStatus _cpuStatus = new();

    [ObservableProperty]
    private HardwareStatus _memoryStatus = new();

    [ObservableProperty]
    private HardwareStatus _diskStatus = new();

    [ObservableProperty]
    private HardwareStatus _systemStatus = new();

    [ObservableProperty]
    private string _welcomeText = "你好，欢迎使用 ScrewDriver Toolbox";

    public DashboardViewModel(IDeviceInfoService deviceInfoService)
    {
        _deviceInfoService = deviceInfoService;
        PageTitle = "首页";
        PageDescription = "系统概览与快捷入口";
    }

    [RelayCommand]
    private void LoadData()
    {
        DeviceInfo = _deviceInfoService.GetDeviceInfo();
        CpuStatus = _deviceInfoService.GetCpuStatus();
        MemoryStatus = _deviceInfoService.GetMemoryStatus();
        DiskStatus = _deviceInfoService.GetDiskStatus();
        SystemStatus = _deviceInfoService.GetSystemStatus();
    }

    [RelayCommand]
    private void QuickOptimize() { }

    [RelayCommand]
    private void QuickRepair() { }

    [RelayCommand]
    private void QuickSetup() { }

    [RelayCommand]
    private void GenerateReport() { }
}
