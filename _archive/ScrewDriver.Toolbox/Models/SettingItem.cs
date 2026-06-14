namespace ScrewDriver.Toolbox.Models;

/// <summary>
/// 系统设置项模型（用于系统优化模块）
/// </summary>
public class SettingItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool RecommendedValue { get; set; }
    public bool IsDangerous { get; set; }
    public string? WarningMessage { get; set; }
    public bool HasPendingChange { get; set; }
    public string? RestoreMethod { get; set; }
}
