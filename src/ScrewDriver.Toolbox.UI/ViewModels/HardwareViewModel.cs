using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ScrewDriver.Toolbox.Core;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Hardware.Services;
using WpfApp = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class HardwareViewModel : BaseViewModel
{
    private readonly HardwareService _hardwareService = new();
    private readonly DispatcherTimer _uptimeTimer;
    private DateTime _bootTime;
    private bool _isLoading;

    // 硬件明细列表
    public ObservableCollection<HardwareDetailItem> HardwareItems { get; } = new();

    // 顶部摘要卡片
    public string SystemModel { get; private set; } = "";
    public string SystemModelDetail { get; private set; } = "";
    public string OsVersion { get; private set; } = "";
    public string OsVersionDetail { get; private set; } = "";
    public string Uptime { get; private set; } = "";
    public string RefreshTime { get; private set; } = "";

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand DetailCommand { get; }
    public ICommand ScreenshotCommand { get; }

    private bool _isActive;

    public HardwareViewModel()
    {
        RefreshCommand = new RelayCommand(_ => Refresh());
        DetailCommand = new RelayCommand(_ => NavigateToDetail());
        ScreenshotCommand = new RelayCommand(_ => TakeScreenshot());

        _uptimeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uptimeTimer.Tick += (_, _) => UpdateUptime();

        Activate();
    }

    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        _uptimeTimer.Start();
        if (HardwareItems.Count == 0)
            _ = LoadDataAsync();
    }

    public void Deactivate()
    {
        if (!_isActive) return;
        _isActive = false;
        _uptimeTimer.Stop();
    }

    private async void Refresh()
    {
        HardwareService.ClearCache();
        HardwareItems.Clear();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            // 1. 同步读取轻量信息（注册表读取，不卡 UI）
            var manufacturer = SystemInfo.DetectHardwareBrand() ?? "未知";
            var model = SystemInfo.DetectSystemModel() ?? "未知";
            SystemModel = manufacturer;
            SystemModelDetail = model;
            OnPropertyChanged(nameof(SystemModel));
            OnPropertyChanged(nameof(SystemModelDetail));

            var os = SystemInfo.WindowsVersionString ?? "Windows";
            OsVersion = os.Contains("10") ? "Windows 10" : os.Contains("11") || os.Contains("10.0.2") ? "Windows 11" : os;
            OsVersionDetail = os;
            OnPropertyChanged(nameof(OsVersion));
            OnPropertyChanged(nameof(OsVersionDetail));

            // 2. 后台线程执行 WMI 查询（含超时保护）
            List<HardwareDetailModule> modules;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                modules = await Task.Run(() => _hardwareService.GetAllDetailInfo(), cts.Token);
            }
            catch (OperationCanceledException)
            {
                modules = new List<HardwareDetailModule>
                {
                    new() { ModuleName = "硬件检测", Icon = "⚠️", Items = new() { new() { Label = "状态", Value = "WMI 查询超时，请刷新重试" } } }
                };
            }
            catch (Exception ex)
            {
                modules = new List<HardwareDetailModule>
                {
                    new() { ModuleName = "硬件检测", Icon = "⚠️", Items = new() { new() { Label = "状态", Value = $"检测失败: {ex.Message}" } } }
                };
            }

            DateTime bootTime;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                bootTime = await Task.Run(() => GetBootTime(), cts.Token);
            }
            catch
            {
                bootTime = DateTime.MinValue;
            }

            // 3. 回到 UI 线程更新
            _bootTime = bootTime;
            UpdateUptime();
            OnPropertyChanged(nameof(Uptime));

            HardwareItems.Clear();
            foreach (var module in modules)
            {
                if (module.Items.Count == 0) continue;

                var important = new List<HardwareDetailItem>();
                foreach (var item in module.Items)
                {
                    if (item.Label.Contains("型号") || item.Label.Contains("总容量") ||
                        item.Label.Contains("核心") || item.Label.Contains("频率") ||
                        item.Label.Contains("分辨率") || item.Label.Contains("显存") ||
                        item.Label.Contains("可用空间") || item.Label.Contains("制造商") ||
                        item.Label == "BIOS 版本" || item.Label == "MAC 地址" ||
                        item.Label == "声卡" || item.Label == "网卡")
                    {
                        important.Add(item);
                    }
                }
                if (important.Count == 0 && module.Items.Count > 0)
                    important.Add(module.Items[0]);
                if (important.Count > 3)
                    important = important.Take(3).ToList();

                HardwareItems.Add(new HardwareDetailItem { Label = module.ModuleName, Value = "", IsHeader = true });
                foreach (var item in important)
                    HardwareItems.Add(item);
            }

            RefreshTime = DateTime.Now.ToString("HH:mm:ss");
            OnPropertyChanged(nameof(RefreshTime));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HardwareViewModel LoadDataAsync failed: {ex.Message}");
            HardwareItems.Clear();
            HardwareItems.Add(new HardwareDetailItem { Label = "错误", Value = $"数据加载异常: {ex.Message}", IsHeader = true });
        }
        finally
        {
            IsLoading = false;
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
        catch { System.Diagnostics.Debug.WriteLine("Hardware load error"); }
        return DateTime.MinValue;
    }

    private static void NavigateToDetail()
    {
        System.Windows.MessageBox.Show("当前页面已展示全部硬件参数。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void TakeScreenshot()
    {
        try
        {
            System.Windows.Clipboard.SetText("截图功能待实现");
        }
        catch { System.Diagnostics.Debug.WriteLine("Hardware load error"); }
    }
}
