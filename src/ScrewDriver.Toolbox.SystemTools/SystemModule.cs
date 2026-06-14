using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.SystemTools;

public class SystemModule : IToolModule
{
    public string Name => "系统优化";
    public string Description => "Windows 更新 / Defender / 隐私设置 / 性能优化";
    public string Category => "系统";
    public string? IconGlyph => "";
}
