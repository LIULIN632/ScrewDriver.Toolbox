using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.SystemTools.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class ScenarioViewModel : BaseViewModel
{
    private readonly SystemOptimizerService _optimizer = new();
    private bool _isRunning;
    private string _currentStepDescription = string.Empty;
    private int _progressValue;
    private string _statusText = string.Empty;

    public ObservableCollection<ScenarioDefinition> Scenarios { get; } = new();

    public bool IsRunning
    {
        get => _isRunning;
        set { SetProperty(ref _isRunning, value); OnPropertyChanged(nameof(IsNotRunning)); }
    }

    public bool IsNotRunning => !_isRunning;

    public string CurrentStepDescription
    {
        get => _currentStepDescription;
        set => SetProperty(ref _currentStepDescription, value);
    }

    public int ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public RelayCommand ExecuteScenarioCommand { get; }

    public ScenarioViewModel()
    {
        foreach (var s in BuildScenarios())
            Scenarios.Add(s);

        ExecuteScenarioCommand = new RelayCommand(async param =>
        {
            if (param is not ScenarioDefinition scenario) return;

            var result = MessageBox.Show(
                $"即将执行「{scenario.Name}」方案，共 {scenario.Steps.Count} 个步骤。\n\n确定继续？",
                "确认执行", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes) return;

            IsRunning = true;
            ProgressValue = 0;
            StatusText = "正在执行...";

            var totalSteps = scenario.Steps.Count;
            var current = 0;

            foreach (var step in scenario.Steps)
            {
                current++;
                var desc = $"[{current}/{totalSteps}] {step.Description}";

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CurrentStepDescription = desc;
                }, DispatcherPriority.Background);

                var ok = await Task.Run(() => ExecuteStep(step));

                if (!ok)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        StatusText = $"步骤 {current} 执行失败，继续执行后续步骤...";
                    }, DispatcherPriority.Background);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProgressValue = current * 100 / totalSteps;
                }, DispatcherPriority.Background);

                await Task.Delay(600); // Small pause so user can see progress
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                IsRunning = false;
                ProgressValue = 100;
                StatusText = $"「{scenario.Name}」执行完成。";

                MessageBox.Show($"「{scenario.Name}」方案执行完成！\n\n建议重启系统以使所有设置生效。",
                    "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }, DispatcherPriority.Background);
        });
    }

    private bool ExecuteStep(ScenarioStep step)
    {
        try
        {
            switch (step.CommandType)
            {
                case "Shell":
                    var psi = new ProcessStartInfo("cmd.exe", $"/c {step.CommandParam}")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psi))
                        p?.WaitForExit(30000);
                    return true;

                case "Optimization":
                    return _optimizer.ApplySetting(step.CommandParam, step.EnableValue);

                case "Repair":
                    return RunRepairCommands(step.CommandParam);

                case "Message":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(step.CommandParam, "提示",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                    return true;

                default:
                    return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private static bool RunRepairCommands(string scenarioName)
    {
        var commands = scenarioName switch
        {
            "网络异常" => new[] { "ipconfig /flushdns", "netsh winsock reset" },
            _ => Array.Empty<string>()
        };

        if (commands.Length == 0) return false;

        try
        {
            foreach (var cmd in commands)
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c {cmd}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(10000);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<ScenarioDefinition> BuildScenarios() => new()
    {
        new ScenarioDefinition
        {
            Name = "新机开荒",
            Description = "验机、系统优化、安装常用软件，快速完成新机配置",
            Icon = "🆕",
            Steps = new List<ScenarioStep>
            {
                new() { CommandType = "Message", CommandParam = "请确保已备份重要数据，建议先连接电源。\n\n即将开始执行新机开荒方案。" },
                new() { CommandType = "Shell", Description = "生成系统健康报告",
                    CommandParam = "perfmon /report" },
                new() { CommandType = "Optimization", Description = "显示文件扩展名",
                    CommandParam = "show-extensions", EnableValue = true },
                new() { CommandType = "Optimization", Description = "关闭遥测",
                    CommandParam = "telemetry", EnableValue = false },
                new() { CommandType = "Repair", Description = "重置网络协议",
                    CommandParam = "网络异常" },
                new() { CommandType = "Shell", Description = "安装 7-Zip",
                    CommandParam = "winget install 7zip.7zip --accept-package-agreements" },
                new() { CommandType = "Shell", Description = "安装 VLC 播放器",
                    CommandParam = "winget install VideoLAN.VLC --accept-package-agreements" },
                new() { CommandType = "Message", CommandParam = "新机开荒完成！\n\n建议重启系统以使所有设置生效。" }
            }
        },
        new ScenarioDefinition
        {
            Name = "游戏加速",
            Description = "关闭后台干扰、禁用 Defender 实时保护、切换高性能电源计划",
            Icon = "🎮",
            Steps = new List<ScenarioStep>
            {
                new() { CommandType = "Message", CommandParam = "即将进行游戏加速优化。\n\nDefender 关闭后系统安全性会降低，建议游戏结束后恢复。" },
                new() { CommandType = "Shell", Description = "切换高性能电源计划",
                    CommandParam = "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" },
                new() { CommandType = "Optimization", Description = "开启游戏模式",
                    CommandParam = "game-mode", EnableValue = true },
                new() { CommandType = "Optimization", Description = "关闭 Defender 实时保护",
                    CommandParam = "disable-defender", EnableValue = true },
                new() { CommandType = "Shell", Description = "禁用遥测服务",
                    CommandParam = "sc config DiagTrack start=disabled" },
                new() { CommandType = "Shell", Description = "关闭 OneDrive",
                    CommandParam = "taskkill /f /im OneDrive.exe 2>nul || exit /b 0" },
                new() { CommandType = "Message", CommandParam = "游戏加速已启用！\n\nDefender 关闭后请注意安全，重启系统后 Defender 会自动恢复。" }
            }
        },
        new ScenarioDefinition
        {
            Name = "系统瘦身",
            Description = "清理临时文件、组件存储、关闭冗余服务",
            Icon = "🧹",
            Steps = new List<ScenarioStep>
            {
                new() { CommandType = "Message", CommandParam = "开始系统瘦身，可能需要几分钟，请耐心等待。" },
                new() { CommandType = "Shell", Description = "磁盘清理",
                    CommandParam = "cleanmgr /sagerun:1 2>nul || cleanmgr /verylowdisk" },
                new() { CommandType = "Shell", Description = "清理组件存储",
                    CommandParam = "dism /online /cleanup-image /startcomponentcleanup" },
                new() { CommandType = "Optimization", Description = "关闭 VBS 降低内存占用",
                    CommandParam = "vbs", EnableValue = false },
                new() { CommandType = "Shell", Description = "禁用 Windows Update 服务",
                    CommandParam = "sc config wuauserv start=disabled" },
                new() { CommandType = "Shell", Description = "清理 Windows 应用缓存",
                    CommandParam = "del /f /s /q %temp%\\*.* 2>nul || exit /b 0" },
                new() { CommandType = "Message", CommandParam = "系统瘦身完成！\n\n建议重启系统，磁盘空间应已有明显释放。" }
            }
        },
        new ScenarioDefinition
        {
            Name = "办公纯净",
            Description = "关闭通知弹窗、专注模式、广告追踪",
            Icon = "📝",
            Steps = new List<ScenarioStep>
            {
                new() { CommandType = "Message", CommandParam = "进入办公纯净模式，将屏蔽系统通知和广告干扰。" },
                new() { CommandType = "Shell", Description = "关闭系统通知",
                    CommandParam = "reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\" /v NOC_GLOBAL_SETTING_TOASTS_ENABLED /t REG_DWORD /d 0 /f" },
                new() { CommandType = "Shell", Description = "关闭开始菜单建议",
                    CommandParam = "reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager\" /v SystemPaneSuggestionsEnabled /t REG_DWORD /d 0 /f" },
                new() { CommandType = "Optimization", Description = "关闭广告 ID",
                    CommandParam = "ad-id", EnableValue = false },
                new() { CommandType = "Shell", Description = "切换节能电源计划",
                    CommandParam = "powercfg /setactive a1841308-3541-4fab-bc81-f71556f20b4a" },
                new() { CommandType = "Message", CommandParam = "办公纯净模式已开启！\n\n系统通知已屏蔽，广告追踪已关闭，电源切换为节能模式。" }
            }
        }
    };
}
