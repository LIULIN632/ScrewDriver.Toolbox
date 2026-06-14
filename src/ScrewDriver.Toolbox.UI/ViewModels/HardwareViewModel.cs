using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Hardware.Services;
using ScrewDriver.Toolbox.UI.Views;
using WpfApp = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class HardwareViewModel : BaseViewModel
{
    private readonly HardwareService _hardwareService = new();

    public ObservableCollection<HardwareComponent> Components { get; } = new();
    public string RefreshTime { get; private set; } = "加载中...";

    // 顶部摘要卡片
    public string SystemModel { get; private set; } = "检测中...";
    public string SystemModelDetail { get; private set; } = "";
    public string OsVersion { get; private set; } = "检测中...";
    public string OsVersionDetail { get; private set; } = "";
    public string Uptime { get; private set; } = "检测中...";
    public string UptimeDetail { get; private set; } = "";

    public ICommand RefreshCommand { get; }
    public ICommand DetailCommand { get; }
    public ICommand ScreenshotCommand { get; }

    public HardwareViewModel()
    {
        RefreshCommand = new RelayCommand(_ => Refresh());
        DetailCommand = new RelayCommand(_ => NavigateToDetail());
        ScreenshotCommand = new RelayCommand(_ => TakeScreenshot());

        LoadData();
    }

    private void Refresh()
    {
        Components.Clear();
        LoadData();
    }

    private void LoadData()
    {
        // 硬件组件
        var all = _hardwareService.GetAllHardwareInfo();
        foreach (var c in all)
            Components.Add(c);

        // 摘要卡片：机型
        var manufacturer = SystemInfo.DetectHardwareBrand() ?? "未知";
        var model = SystemInfo.DetectSystemModel() ?? "未知";
        SystemModel = manufacturer;
        SystemModelDetail = model;
        OnPropertyChanged(nameof(SystemModel));
        OnPropertyChanged(nameof(SystemModelDetail));

        // 摘要卡片：系统版本
        var os = SystemInfo.WindowsVersionString ?? "Windows";
        OsVersion = os.Contains("10") ? "Windows 10" : os.Contains("11") || os.Contains("10.0.2") ? "Windows 11" : os;
        OsVersionDetail = os;
        OnPropertyChanged(nameof(OsVersion));
        OnPropertyChanged(nameof(OsVersionDetail));

        // 摘要卡片：运行时长
        Uptime = HardwareService.GetSystemUptime();
        UptimeDetail = $"上次启动后至今";
        OnPropertyChanged(nameof(Uptime));
        OnPropertyChanged(nameof(UptimeDetail));

        RefreshTime = DateTime.Now.ToString("HH:mm:ss");
        OnPropertyChanged(nameof(RefreshTime));
    }

    private static void NavigateToDetail()
    {
        if (WpfApp.Current.MainWindow is MainWindow main)
        {
            main.MainFrame.Navigate(new HardwareDetailPage());
        }
    }

    private static void TakeScreenshot()
    {
        // 简单截图：弹出保存对话框或复制到剪贴板
        try
        {
            System.Windows.Clipboard.SetText("截图功能待实现");
        }
        catch { }
    }
}
