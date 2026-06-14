namespace ScrewDriver.Toolbox.Core.Models;

public class HardwareComponent
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string PrimaryInfo { get; set; } = string.Empty;
    public string SecondaryInfo { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public string HealthLevel { get; set; } = "normal";
    public string TipText { get; set; } = string.Empty;
}
