using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.Hardware;

public class HardwareModule : IToolModule
{
    public string Name => "硬件信息";
    public string Description => "CPU / GPU / 内存 / 磁盘 / 温度传感器信息";
    public string Category => "硬件";
    public string? IconGlyph => "";
}
