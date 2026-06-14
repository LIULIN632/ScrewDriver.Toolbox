using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScrewDriver.Toolbox.Services;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class RepairCenterViewModel : BaseViewModel
{
    private readonly ISystemRepairService _repairService;

    [ObservableProperty]
    private ObservableCollection<string> _problems = [];

    [ObservableProperty]
    private string _selectedProblem = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RepairSolution> _solutions = [];

    [ObservableProperty]
    private RepairSolution? _selectedSolution;

    [ObservableProperty]
    private string _diagnosticInfo = string.Empty;

    public Dictionary<string, string> ProblemNames { get; } = new()
    {
        ["system_slow"] = "系统卡顿",
        ["network_issue"] = "网络异常",
        ["bsod"] = "蓝屏分析",
        ["update_failed"] = "更新失败",
        ["game_lag"] = "游戏掉帧",
        ["app_crash"] = "软件打不开"
    };

    public RepairCenterViewModel(ISystemRepairService repairService)
    {
        _repairService = repairService;
        PageTitle = "系统修复";
        PageDescription = "快速诊断并解决常见系统问题";

        Problems = new ObservableCollection<string>(repairService.GetProblemCategories());
    }

    partial void OnSelectedProblemChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Solutions = new ObservableCollection<RepairSolution>(_repairService.GetSolutionsForProblem(value));
        }
    }

    [RelayCommand]
    private void ExecuteSolution(RepairSolution solution)
    {
        _repairService.ExecuteSolution(solution);
    }

    [RelayCommand]
    private void RunDiagnostics()
    {
        DiagnosticInfo = _repairService.GetDiagnosticInfo();
    }

    public string GetProblemDisplayName(string key) =>
        ProblemNames.TryGetValue(key, out var name) ? name : key;
}
