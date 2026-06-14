namespace ScrewDriver.Toolbox.Services;

/// <summary>
/// 系统修复服务
/// </summary>
public interface ISystemRepairService
{
    List<string> GetProblemCategories();
    List<RepairSolution> GetSolutionsForProblem(string problemId);
    bool ExecuteSolution(RepairSolution solution);
    string GetDiagnosticInfo();
}

public class RepairSolution
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public bool RequiresAdmin { get; set; } = true;
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Safe;
}
