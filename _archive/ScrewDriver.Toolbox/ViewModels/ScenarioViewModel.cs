using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScrewDriver.Toolbox.Models;
using ScrewDriver.Toolbox.Services;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class ScenarioViewModel : BaseViewModel
{
    private readonly ISystemOptimizerService _optimizerService;
    private readonly ISystemRepairService _repairService;

    [ObservableProperty]
    private ObservableCollection<ScenarioModel> _scenarios = [];

    [ObservableProperty]
    private ScenarioModel? _selectedScenario;

    public ScenarioViewModel(ISystemOptimizerService optimizerService, ISystemRepairService repairService)
    {
        _optimizerService = optimizerService;
        _repairService = repairService;
        PageTitle = "场景方案";
        PageDescription = "一键执行预设方案，快速完成常见任务";

        InitializeScenarios();
    }

    private void InitializeScenarios()
    {
        Scenarios =
        [
            new ScenarioModel
            {
                Id = "new_pc", Name = "新机开荒", Description = "验机检测 + 系统优化 + 常用软件安装 + 备份设置",
                Icon = "🚀", EstimatedTime = "约 15 分钟",
                Steps =
                [
                    new ScenarioStep { Name = "创建还原点", Description = "创建系统还原点以便恢复" },
                    new ScenarioStep { Name = "系统优化", Description = "优化系统设置提升性能" },
                    new ScenarioStep { Name = "安装常用软件", Description = "安装精选常用工具" },
                    new ScenarioStep { Name = "生成验机报告", Description = "生成硬件检测报告" }
                ]
            },
            new ScenarioModel
            {
                Id = "game", Name = "游戏模式", Description = "高性能策略 + 关闭后台干扰 + 优化系统设置",
                Icon = "🎮", EstimatedTime = "约 5 分钟",
                Steps =
                [
                    new ScenarioStep { Name = "切换高性能电源", Description = "切换到高性能电源计划" },
                    new ScenarioStep { Name = "关闭后台干扰", Description = "关闭不必要的后台应用" },
                    new ScenarioStep { Name = "优化系统设置", Description = "关闭影响游戏性能的功能" }
                ]
            },
            new ScenarioModel
            {
                Id = "office", Name = "办公模式", Description = "平衡功耗 + 安静策略 + 专注模式",
                Icon = "💼", EstimatedTime = "约 3 分钟",
                Steps =
                [
                    new ScenarioStep { Name = "平衡模式", Description = "切换到平衡电源计划" },
                    new ScenarioStep { Name = "降低功耗", Description = "优化电源效率" },
                    new ScenarioStep { Name = "开启专注模式", Description = "启用系统专注模式减少干扰" }
                ]
            },
            new ScenarioModel
            {
                Id = "repair", Name = "修复模式", Description = "系统扫描 + 网络修复 + 日志导出",
                Icon = "🩺", EstimatedTime = "约 10 分钟",
                Steps =
                [
                    new ScenarioStep { Name = "系统扫描", Description = "运行 SFC 和 DISM 扫描系统" },
                    new ScenarioStep { Name = "网络修复", Description = "重置网络协议栈" },
                    new ScenarioStep { Name = "日志导出", Description = "导出系统诊断日志" }
                ]
            },
            new ScenarioModel
            {
                Id = "install", Name = "装机模式", Description = "一键安装常用软件 + 一键更新工具库",
                Icon = "📦", EstimatedTime = "约 20 分钟",
                Steps =
                [
                    new ScenarioStep { Name = "安装常用软件", Description = "批量安装精选工具" },
                    new ScenarioStep { Name = "更新工具库", Description = "更新内置工具到最新版本" }
                ]
            }
        ];
    }

    [RelayCommand]
    private void SelectScenario(ScenarioModel scenario)
    {
        SelectedScenario = scenario;
    }

    [RelayCommand]
    private void ExecuteScenario(ScenarioModel scenario)
    {
        // TODO: 按步骤依次执行场景方案
    }
}
