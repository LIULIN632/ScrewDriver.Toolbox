using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ScrewDriver.Toolbox.Core;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Hardware.Services;
using ScrewDriver.Toolbox.UI.Views;
using WpfApp = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class HardwareViewModel : BaseViewModel
{
    private readonly HardwareService _hardwareService = new();
    private readonly DispatcherTimer _uptimeTimer;
    private DateTime _bootTime;

    // 硬件明细列表
    public ObservableCollection<HardwareDetailItem> HardwareItems { get; } = new();

    // 顶部摘要卡片
    public string SystemModel { get; private set; } = "检测中...";
    public string SystemModelDetail { get; private set; } = "";
    public string OsVersion { get; private set; } = "检测中...";
    public string OsVersionDetail { get; private set; } = "";
    public string Uptime { get; private set; } = "检测中...";
    public string RefreshTime { get; private set; } = "";

    public ICommand RefreshCommand { get; }
    public ICommand DetailCommand { get; }
    public ICommand ScreenshotCommand { get; }

    public HardwareViewModel()
    {
        RefreshCommand = new RelayCommand(_ => Refresh());
        DetailCommand = new RelayCommand(_ => NavigateToDetail());
        ScreenshotCommand = new RelayCommand(_ => TakeScreenshot());

        _uptimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uptimeTimer.Tick += (_, _) => UpdateUptime();
        _uptimeTimer.Start();

        LoadData();
    }

    private void Refresh()
    {
        HardwareItems.Clear();
        LoadData();
    }

    private void LoadData()
    {
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
        _bootTime = GetBootTime();
        UpdateUptime();
        OnPropertyChanged(nameof(Uptime));

        // 硬件明细
        RefreshTime = DateTime.Now.ToString("HH:mm:ss");
        OnPropertyChanged(nameof(RefreshTime));

        // 从 GetAllDetailInfo 展开为平面列表（每个模块取首条关键信息）
        var modules = _hardwareService.GetAllDetailInfo();
        HardwareItems.Clear();

        foreach (var module in modules)
        {
            if (module.Items.Count == 0) continue;
            // 模块标题行
            HardwareItems.Add(new HardwareDetailItem { Label = $"【{module.ModuleName}】", Value = "" });
            foreach (var item in module.Items)
            {
                HardwareItems.Add(item);
            }
        }
    }

    // 运行时长每秒刷新
    private void UpdateUptime()
    {
        if (_bootTime == default) return;
        var span = DateTime.Now - _bootTime;
        Uptime = span.Days > 0
            ? $"{span.Days} 天 {span.Hours} 小时 {span.Minutes} 分钟 {span.Seconds} 秒"
            : $"{span.Hours} 小时 {span.Minutes} 分钟 {span.Seconds} 秒";
        OnPropertyChanged(nameof(Uptime));
    }

    private static DateTime GetBootTime()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                if (obj["LastBootUpTime"] is string dmtf && dmtf.Length >= 14)
                    return new DateTime(
                        int.Parse(dmtf[..4]), int.Parse(dmtf[4..6]), int.Parse(dmtf[6..8]),
                        int.Parse(dmtf[8..10]), int.Parse(dmtf[10..12]), int.Parse(dmtf[12..14]));
            }
        }
        catch { }
        return DateTime.MinValue;
    }

    private static void NavigateToDetail()
    {
        if (WpfApp.Current.MainWindow is MainWindow main)
            main.MainFrame.Navigate(new HardwareDetailPage());
    }

    private static void TakeScreenshot()
    {
        try
        {
            System.Windows.Clipboard.SetText("截图功能待实现");
        }
        catch { }
    }
}
