namespace ScrewDriver.Toolbox.Core.Models;

public class HardwareDetailModule
{
    public string ModuleName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<HardwareDetailItem> Items { get; set; } = new();
}

public class HardwareDetailItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
