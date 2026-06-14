using System.Collections.ObjectModel;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Hardware.Services;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class HardwareViewModel : BaseViewModel
{
    private readonly HardwareService _hardwareService = new();

    public ObservableCollection<HardwareComponent> Components { get; } = new();
    public string RefreshTime { get; private set; } = "加载中...";
    public string SystemSummary { get; private set; } = "正在检测...";

    public RelayCommand RefreshCommand { get; }

    public HardwareViewModel()
    {
        RefreshCommand = new RelayCommand(_ =>
        {
            Components.Clear();
            LoadHardware();
        });
        LoadHardware();
    }

    private void LoadHardware()
    {
        var all = _hardwareService.GetAllHardwareInfo();
        foreach (var c in all)
            Components.Add(c);

        var cpu = all.FirstOrDefault(c => c.Name == "CPU")?.PrimaryInfo ?? "未知";
        var mem = all.FirstOrDefault(c => c.Name == "内存")?.PrimaryInfo ?? "未知";
        var disk = all.FirstOrDefault(c => c.Name == "磁盘")?.PrimaryInfo ?? "未知";
        var gpu = all.FirstOrDefault(c => c.Name == "GPU")?.PrimaryInfo ?? "未知";

        SystemSummary = $"CPU: {cpu}  |  内存: {mem}  |  磁盘: {disk}  |  GPU: {gpu}";
        OnPropertyChanged(nameof(SystemSummary));

        RefreshTime = DateTime.Now.ToString("HH:mm:ss");
        OnPropertyChanged(nameof(RefreshTime));
    }
}
