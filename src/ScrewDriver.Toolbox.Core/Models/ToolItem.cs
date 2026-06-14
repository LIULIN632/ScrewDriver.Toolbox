namespace ScrewDriver.Toolbox.Core.Models;

public class ToolItem
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OfficialUrl { get; set; } = string.Empty;
    public string WingetId { get; set; } = string.Empty;
    public string GithubUrl { get; set; } = string.Empty;
    public string LaunchPath { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "安全";
    public bool IsInstalled { get; set; }
    public string? IconPath { get; set; }
    public string? LocalExePath { get; set; }
    public bool IsCustom { get; set; }
    public bool HasUpdate { get; set; }
    public bool IsPinned { get; set; }
}
