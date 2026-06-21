using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class SystemUIDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        // 系统界面
        ("enable-numlock", "开机自动开启 NumLock", "登录界面自动打开数字小键盘 NumLock",
            "系统界面", RiskLevel.Recommended, "仅改变 NumLock 默认状态，无副作用", "将 InitialKeyboardIndicators 改回 0 即可关闭",
            RegistryHive.CurrentUser, @"Control Panel\Keyboard", "InitialKeyboardIndicators", "2", "0", RegistryValueKind.String,
            "Registry", null, null),

        ("disable-sticky-keys", "禁用粘滞键", "关闭 Shift 连按 5 次触发的粘滞键功能",
            "系统界面", RiskLevel.Recommended, "仅禁用粘滞键，不影响其他辅助功能。可避免游戏中误触弹出对话框", "将 Flags 改回 510 即可恢复",
            RegistryHive.CurrentUser, @"Control Panel\Accessibility\StickyKeys", "Flags", "506", "510", RegistryValueKind.String,
            "Registry", null, null),

        ("disable-filter-keys", "禁用筛选键", "关闭右 Shift 长按触发的筛选键功能",
            "系统界面", RiskLevel.Recommended, "仅禁用筛选键，不影响其他辅助功能", "将 Flags 改回 122 即可恢复",
            RegistryHive.CurrentUser, @"Control Panel\Accessibility\Keyboard Response", "Flags", "58", "122", RegistryValueKind.String,
            "Registry", null, null),

        ("disable-toggle-keys", "禁用切换键", "关闭 NumLock/CapsLock 切换时的提示音",
            "系统界面", RiskLevel.Optional, "仅禁用切换提示音，不影响按键功能", "将 Flags 改回 63 即可恢复",
            RegistryHive.CurrentUser, @"Control Panel\Accessibility\ToggleKeys", "Flags", "58", "63", RegistryValueKind.String,
            "Registry", null, null),

        RegToggle("scroll-direction", "自然滚动方向", "触控板/鼠标滚轮反向：向下滚动时页面向上（类似 macOS 自然滚动）",
            "系统界面", RiskLevel.Optional, "仅改变滚动方向，不影响其他输入功能。习惯 macOS 的用户更易适应", "将 FlipFlopWheel 改回 0 即可恢复默认方向",
            RegistryHive.CurrentUser, @"Control Panel\Desktop", "FlipFlopWheel"),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["enable-numlock"] = RecommendedAction.Enable,
        ["disable-sticky-keys"] = RecommendedAction.Enable,
        ["disable-filter-keys"] = RecommendedAction.Enable,
        ["disable-toggle-keys"] = RecommendedAction.Enable,
        ["scroll-direction"] = RecommendedAction.None,
    };
}
