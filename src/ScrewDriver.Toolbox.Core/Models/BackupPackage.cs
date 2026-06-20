namespace ScrewDriver.Toolbox.Core.Models;

public class SettingSnapshot
{
    public string Key { get; set; } = string.Empty;
    public string RegistryPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public object? OriginalValue { get; set; }
    public object? CurrentValue { get; set; }
    public DateTime ModifyTime { get; set; }
}

public class BackupPackage
{
    public string BackupId { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public int ItemCount { get; set; }
    public List<SettingSnapshot> Snapshots { get; set; } = new();
    public Dictionary<string, object> AppConfig { get; set; } = new();
}
