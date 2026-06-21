using ScrewDriver.Toolbox.SystemTools.Services;

namespace ScrewDriver.Toolbox.UI.Common;

/// <summary>统一设置读写入口，封装 SystemOptimizerService + RegistryOptimizer</summary>
public static class RegistryHelper
{
    private static readonly SystemOptimizerService _service = new();

    public static bool ApplySettingById(string id, bool enable)
    {
        // 先尝试通过 SystemOptimizerService 应用
        if (_service.ApplySetting(id, enable))
            return true;

        // 如果服务不支持，尝试直接注册表操作
        return RegistryOptimizer.ApplySettingById(id, enable);
    }
}
