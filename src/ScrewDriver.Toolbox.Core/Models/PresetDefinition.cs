namespace ScrewDriver.Toolbox.Core.Models;

public class PresetDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public Dictionary<string, bool> TargetStates { get; set; } = new();
}
