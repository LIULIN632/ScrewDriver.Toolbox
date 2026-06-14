using System.Collections.ObjectModel;
using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.Core.Models;

public enum RiskLevel
{
    Recommended,
    Optional,
    Dangerous
}

public enum RecommendedAction
{
    None,
    Enable,
    Disable
}

public class SystemSettingItem : IRiskOperation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string RiskDescription { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public RecommendedAction Recommendation { get; set; }
    public string RecommendationText => Recommendation switch
    {
        RecommendedAction.Enable => "推荐开启",
        RecommendedAction.Disable => "推荐关闭",
        _ => ""
    };
    public bool CanRevert { get; set; } = true;
    public string RevertMethodDescription { get; set; } = string.Empty;
    public string OperationType { get; set; } = "Registry";
    public string? EnablePsCommand { get; set; }
    public string? DisablePsCommand { get; set; }
}

public class SettingGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public ObservableCollection<SystemSettingItem> Items { get; set; } = new();
}
