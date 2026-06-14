namespace ScrewDriver.Toolbox.Models;

/// <summary>
/// 工具数据模型
/// </summary>
public class ToolModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // 来源
    public string? OfficialUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WingetCommand { get; set; }
    public string? ScoopCommand { get; set; }

    // 版本信息
    public string? Version { get; set; }
    public string? LastUpdated { get; set; }

    // 安全校验
    public string? Sha256 { get; set; }
    public bool HasDigitalSignature { get; set; }

    // 评级
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Safe;
    public int RecommendScore { get; set; } = 3;

    // 是否已安装
    public bool IsInstalled { get; set; }
}

public enum RiskLevel
{
    Safe = 0,
    Low = 1,
    Warning = 2,
    Dangerous = 3
}
