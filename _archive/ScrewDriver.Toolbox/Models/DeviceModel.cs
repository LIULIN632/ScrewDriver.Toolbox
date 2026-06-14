namespace ScrewDriver.Toolbox.Models;

/// <summary>
/// 设备信息数据模型
/// </summary>
public class DeviceModel
{
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string CpuName { get; set; } = string.Empty;
    public string GpuName { get; set; } = string.Empty;
    public int MemoryGB { get; set; }
    public string DiskInfo { get; set; } = string.Empty;
    public string WindowsVersion { get; set; } = string.Empty;
    public string BiosVersion { get; set; } = string.Empty;
    public string OsBuild { get; set; } = string.Empty;
    public TimeSpan UpTime { get; set; }
}

/// <summary>
/// 硬件实时状态
/// </summary>
public class HardwareStatus
{
    public string ComponentName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public StatusColor StatusColor { get; set; } = StatusColor.Normal;
}

public enum StatusColor
{
    Normal,
    Warning,
    Critical
}
