using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class RepairCenterViewModel : BaseViewModel
{
    private readonly RecentToolsService _recentService = new();

    public ObservableCollection<RepairScenario> Scenarios { get; } = new();
    public ObservableCollection<ToolItem> InstalledTools { get; } = new();

    public RelayCommand ExecuteRepairCommand { get; }
    public RelayCommand LaunchToolCommand { get; }
    public RelayCommand DetectCommand { get; }

    private static readonly HashSet<string> _repairCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "系统工具", "安全工具", "CPU工具", "主板工具", "内存工具",
        "显卡工具", "硬盘工具", "屏幕工具", "外设工具",
        "联想工具", "华硕工具", "惠普工具", "戴尔工具",
        "微星工具", "机械革命", "通用工具",
        "游戏工具", "烤机工具", "音视频播放器", "音视频处理工具",
        "图像与设计工具"
    };

    public RepairCenterViewModel()
    {
        foreach (var s in BuildScenarios())
            Scenarios.Add(s);

        RefreshInstalledTools();

        DetectCommand = new RelayCommand(async param =>
        {
            if (param is RepairScenario scenario)
                await DetectAsync(scenario);
        });

        ExecuteRepairCommand = new RelayCommand(async param =>
        {
            if (param is not RepairScenario scenario) return;

            // 点击修复时先检测
            if (scenario.Status != "检测到问题")
            {
                await DetectAsync(scenario);
                if (scenario.Status != "检测到问题")
                {
                    MessageBox.Show("当前未发现问题，无需修复。", "无需修复",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            var result = MessageBox.Show(
                $"即将执行「{scenario.Name}」修复方案，共 {scenario.Commands.Count} 个步骤。\n\n确定继续？",
                "确认修复", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            foreach (var cmd in scenario.Commands)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c {cmd}")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    })?.WaitForExit(10000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"执行失败: {cmd}\n\n{ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            _recentService.AddTool(scenario.Name, "RepairCenterPage");
            MessageBox.Show($"「{scenario.Name}」修复完成。", "完成",
                MessageBoxButton.OK, MessageBoxImage.Information);
        });

        LaunchToolCommand = new RelayCommand(param =>
        {
            if (param is not ToolItem tool) return;

            string? path = tool.LocalExePath ?? tool.LaunchPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show($"工具文件缺失\n\n{tool.Name} 的程序文件未找到。",
                    "启动失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                _recentService.AddTool(tool.Name, "RepairCenterPage");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        InstalledToolsCache.Instance.CacheUpdated += () =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(RefreshInstalledTools);
        };
    }

    private void RefreshInstalledTools()
    {
        var tools = InstalledToolsCache.Instance.InstalledTools
            .Where(t => _repairCategories.Contains(t.Category))
            .ToList();

        InstalledTools.Clear();
        foreach (var t in tools)
            InstalledTools.Add(t);
    }

    private static async Task DetectAsync(RepairScenario scenario)
    {
        scenario.Status = "检测中...";
        scenario.StatusColor = "#2563EB";

        var hasIssue = false;
        foreach (var cmd in scenario.DetectCommands)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c {cmd}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = Process.Start(psi);
                if (p != null)
                {
                    var output = await p.StandardOutput.ReadToEndAsync();
                    await p.WaitForExitAsync();
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        hasIssue = true;
                        break;
                    }
                }
            }
            catch { }
        }

        scenario.Status = hasIssue ? "检测到问题" : "一切正常";
        scenario.StatusColor = hasIssue ? "#DC2626" : "#22C55E";
    }

    private static List<RepairScenario> BuildScenarios() => new()
    {
        new()
        {
            Name = "网络异常",
            Cause = "DNS 缓存过期或网络协议栈异常",
            Description = "刷新 DNS 缓存并重置网络协议栈",
            DetectCommands = new List<string> { "ping -n 1 223.5.5.5 | findstr TTL || echo 网络不通" },
            Commands = new List<string> { "ipconfig /flushdns", "netsh winsock reset" }
        },
        new()
        {
            Name = "系统卡顿",
            Cause = "临时文件堆积或磁盘空间不足",
            Description = "打开磁盘清理工具和临时文件夹",
            DetectCommands = new List<string> { "wmic logicaldisk where size>0 get deviceid,freespace,size | findstr /i c:" },
            Commands = new List<string> { "start cleanmgr.exe", "start %temp%" }
        },
        new()
        {
            Name = "蓝屏分析",
            Cause = "驱动冲突或硬件故障",
            Description = "打开事件查看器排查系统错误日志",
            DetectCommands = new List<string> { "wevtutil qe System /c:1 /f:text /q:\"*[System[Level=2]]\" | findstr . || echo 无严重错误" },
            Commands = new List<string> { "start eventvwr.msc" }
        },
        new()
        {
            Name = "更新失败",
            Cause = "Windows Update 缓存异常",
            Description = "重置 Windows Update 服务和缓存",
            DetectCommands = new List<string> { "sc query wuauserv | findstr RUNNING || echo 服务未运行" },
            Commands = new List<string> { "net stop wuauserv", "net start wuauserv" }
        },
        new()
        {
            Name = "游戏掉线",
            Cause = "DNS 解析延迟或网络协议栈异常",
            Description = "刷新 DNS 缓存并重置网络协议栈，切换高性能电源计划",
            DetectCommands = new List<string> { "ping -n 2 223.5.5.5 | findstr 平均", "ping -n 2 180.76.76.76 | findstr 平均" },
            Commands = new List<string> { "ipconfig /flushdns", "netsh winsock reset", "netsh int ip reset", "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" }
        },
        new()
        {
            Name = "软件打不开",
            Cause = "文件关联错误或默认程序设置异常",
            Description = "打开默认程序设置面板",
            DetectCommands = new List<string> { "assoc .txt | findstr txtfile || echo 文件关联异常" },
            Commands = new List<string> { "start control /name Microsoft.DefaultPrograms" }
        }
    };
}
