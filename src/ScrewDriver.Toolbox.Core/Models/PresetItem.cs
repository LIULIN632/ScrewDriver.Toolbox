using System.Collections.ObjectModel;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Models;

public class PresetItem
{
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public string Scene { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string Notice { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public List<string> SettingKeys { get; set; } = new();
    public List<SystemSettingItem> SettingList { get; set; } = new();
    public ICommand? ApplyCommand { get; set; }
    public ICommand? ViewDetailCommand { get; set; }
}
