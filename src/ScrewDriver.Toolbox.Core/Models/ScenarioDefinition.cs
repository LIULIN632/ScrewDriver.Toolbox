namespace ScrewDriver.Toolbox.Core.Models;

public class ScenarioStep
{
    public string Description { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty; // Shell / Optimization / Repair / Message
    public string CommandParam { get; set; } = string.Empty;
    public bool EnableValue { get; set; } // For Optimization commands: true=enable, false=disable
}

public class ScenarioDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<ScenarioStep> Steps { get; set; } = new();
}
