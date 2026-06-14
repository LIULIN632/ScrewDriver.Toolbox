namespace ScrewDriver.Toolbox.Core.Models;

public class RepairScenario
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public List<string> Commands { get; set; } = new();
}
