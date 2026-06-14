using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.Models;

/// <summary>
/// 场景方案数据模型
/// </summary>
public class ScenarioModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public ObservableCollection<ScenarioStep> Steps { get; set; } = [];

    public RiskLevel RiskLevel { get; set; } = RiskLevel.Safe;
    public string? EstimatedTime { get; set; }
}

public class ScenarioStep
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDestructive { get; set; }
    public string? RestoreMethod { get; set; }
}
