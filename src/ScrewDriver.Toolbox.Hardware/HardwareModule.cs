using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.Hardware;

public class HardwareModule : IToolModule
{
    public string Name => "硬件检测";
    public string Description => "CPU / GPU / 内存 / 磁盘 / 温度监控";
    public string Category => "硬件";
    public string? IconGlyph => "";
}
