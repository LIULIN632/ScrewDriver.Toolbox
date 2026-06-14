using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class RepairCenterViewModel : BaseViewModel
{
    private readonly RecentToolsService _recentService = new();

    public ObservableCollection<RepairScenario> Scenarios { get; } = new();

    public RelayCommand ExecuteRepairCommand { get; }

    public RepairCenterViewModel()
    {
        foreach (var s in BuildScenarios())
            Scenarios.Add(s);

        ExecuteRepairCommand = new RelayCommand(param =>
        {
            if (param is not RepairScenario scenario) return;

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
    }

    private static List<RepairScenario> BuildScenarios() => new()
    {
        new()
        {
            Name = "网络异常",
            Cause = "DNS 缓存过期或网络协议栈异常",
            Description = "刷新 DNS 缓存并重置网络协议栈",
            Commands = new List<string> { "ipconfig /flushdns", "netsh winsock reset" }
        },
        new()
        {
            Name = "系统卡顿",
            Cause = "临时文件堆积或磁盘空间不足",
            Description = "打开磁盘清理工具和临时文件夹",
            Commands = new List<string> { "start cleanmgr.exe", "start %temp%" }
        },
        new()
        {
            Name = "蓝屏分析",
            Cause = "驱动冲突或硬件故障",
            Description = "打开事件查看器排查系统错误日志",
            Commands = new List<string> { "start eventvwr.msc" }
        },
        new()
        {
            Name = "更新失败",
            Cause = "Windows Update 缓存异常",
            Description = "重置 Windows Update 授权和缓存",
            Commands = new List<string> { "net stop wuauserv", "net start wuauserv" }
        },
        new()
        {
            Name = "游戏掉帧",
            Cause = "电源计划为节能模式或后台进程干扰",
            Description = "切换电源计划为高性能模式",
            Commands = new List<string> { "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" }
        },
        new()
        {
            Name = "软件打不开",
            Cause = "文件关联错误或默认程序设置异常",
            Description = "打开默认程序设置面板",
            Commands = new List<string> { "start control /name Microsoft.DefaultPrograms" }
        }
    };
}
