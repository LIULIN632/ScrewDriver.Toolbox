using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.Utils;

public class UtilsModule : IToolModule
{
    public string Name => "实用工具";
    public string Description => "常用系统工具 / 效率工具 / 开发工具 / 网络工具";
    public string Category => "工具";
    public string? IconGlyph => "";
}
