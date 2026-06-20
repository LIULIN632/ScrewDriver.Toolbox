using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class PerformanceDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        ("game-mode", "游戏模式", "启用 Windows 游戏模式，优化游戏性能",
         "性能与电源", RiskLevel.Recommended, "游戏模式可能影响后台应用的运行", "将 AllowAutoGameMode 改回 0 即可关闭",
         RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("vbs", "关闭 VBS 内存完整性", "禁用基于虚拟化的安全（VBS）和内存完整性检查",
         "性能与电源", RiskLevel.Dangerous, "VBS 是重要安全特性，关闭后可能降低系统安全性。⚠️ 修改后需重启系统才能生效", "将 EnableVirtualizationBasedSecurity 改回 1 并重启即可重新启用",
         RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("power-plan-high", "高性能电源计划", "切换至高性能电源模式，最大化 CPU 性能",
         "性能与电源", RiskLevel.Optional, "高性能模式会增加耗电和发热，笔记本电池续航缩短", "选择其他电源计划即可恢复",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
         null),

        ("power-plan-balanced", "平衡电源计划", "切换至平衡电源模式，兼顾性能与功耗",
         "性能与电源", RiskLevel.Recommended, "默认推荐设置，适合大多数使用场景", "选择其他电源计划即可恢复",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "powercfg /setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
         null),

        ("power-plan-saver", "节能电源计划", "切换至节能电源模式，最大化电池续航",
         "性能与电源", RiskLevel.Optional, "节能模式会显著降低 CPU 性能，影响使用体验", "选择其他电源计划即可恢复",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "powercfg /setactive a1841308-3541-4fab-bc81-f71556f20b4a",
         null),

        ("disable-autoplay", "关闭自动播放", "禁止插入U盘/光盘时自动弹出播放窗口",
         "性能与电源", RiskLevel.Recommended, "关闭后可防止恶意软件通过自动播放传播", "将 DisableAutoplay 改回 0 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers", "DisableAutoplay", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-sysmain", "关闭 SysMain 服务", "停止并禁用 SysMain（原 Superfetch）预读服务",
         "性能与电源", RiskLevel.Optional, "SSD 硬盘关闭后可减少磁盘写入，HDD 建议保留", "将 SysMain 服务启动类型改回 Auto 即可",
         RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SysMain", "Start", 4, 2, RegistryValueKind.DWord,
         "PowerShell",
         "Stop-Service SysMain -Force; Set-Service SysMain -StartupType Disabled",
         "Set-Service SysMain -StartupType Auto; Start-Service SysMain"),

        ("disable-wsearch", "关闭 Windows Search 索引", "停止并禁用 Windows 搜索索引服务",
         "性能与电源", RiskLevel.Optional, "关闭后搜索速度变慢，但可降低磁盘和 CPU 占用", "将 WSearch 服务启动类型改回 Auto 即可",
         RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSearch", "Start", 4, 2, RegistryValueKind.DWord,
         "PowerShell",
         "Stop-Service WSearch -Force; Set-Service WSearch -StartupType Disabled",
         "Set-Service WSearch -StartupType Auto; Start-Service WSearch"),

        ("disable-hibernate", "关闭系统休眠", "删除休眠文件 (hiberfil.sys)，释放磁盘空间（等于内存大小）",
         "性能与电源", RiskLevel.Optional, "⚠️ 将禁用快速启动（影响开机速度），无法使用休眠和部分节能睡眠模式。SSD 用户建议关闭以释放空间", "执行 'powercfg -h on' 即可重新启用",
         RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0, 1, RegistryValueKind.DWord,
         "PowerShell",
         "powercfg -h off",
         "powercfg -h on"),

        ("disable-background-apps", "关闭后台应用", "禁止所有应用在后台运行（全局隐私 → 后台应用）",
         "性能与电源", RiskLevel.Optional, "关闭后部分应用的通知和实时更新可能延迟", "将 GlobalUserDisabled 改回 0 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-visual-effects", "关闭视觉特效", "调整为「最佳性能」模式，关闭动画/阴影/平滑字体等视觉效果",
         "性能与电源", RiskLevel.Optional, "不建议日常办公场景开启，会明显降低视觉体验", "将 VisualFXSetting 改回 1（最佳外观）或 0（自定义）",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        RegToggle("disable-prefetch", "关闭预读取", "禁用 Windows Prefetch 预读取功能",
            "性能与电源", RiskLevel.Optional, "SSD 硬盘建议关闭以减少磁盘写入，HDD 建议保留以提升启动速度", "将 EnablePrefetcher 改回 3 即可恢复",
            RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 0, 3),

        RegToggle("enable-long-paths", "启用长路径支持", "允许文件路径超过 260 个字符限制（需应用支持）",
            "性能与电源", RiskLevel.Optional, "部分老旧应用可能不兼容超长路径", "将 LongPathsEnabled 改回 0 即可",
            RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled"),

        RegToggle("disable-notification-tips", "关闭通知区域提示", "禁用任务栏通知区域的提示气泡",
            "性能与电源", RiskLevel.Optional, "关闭后可能错过部分系统提示，但减少干扰", "将 EnableBalloonTips 改回 1 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableBalloonTips", 0, 1),

        ("disable-xbox-services", "关闭 Xbox 服务", "停止并禁用 Xbox 相关后台服务（XblAuthManager/XblGameSave/XboxNetApiSvc/XboxGipSvc）",
            "性能与电源", RiskLevel.Optional, "非游戏用户建议关闭，可减少后台资源占用。游戏用户请保留", "将各 XBox 服务恢复为 Manual 启动类型",
            RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
            "PowerShell",
            "Get-Service XblAuthManager,XblGameSave,XboxNetApiSvc,XboxGipSvc -ErrorAction SilentlyContinue | Stop-Service -Force; Get-Service XblAuthManager,XblGameSave,XboxNetApiSvc,XboxGipSvc -ErrorAction SilentlyContinue | Set-Service -StartupType Disabled",
            "Get-Service XblAuthManager,XblGameSave,XboxNetApiSvc,XboxGipSvc -ErrorAction SilentlyContinue | Set-Service -StartupType Manual"),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["game-mode"] = RecommendedAction.None,
        ["vbs"] = RecommendedAction.Enable,
        ["power-plan-high"] = RecommendedAction.None,
        ["power-plan-balanced"] = RecommendedAction.None,
        ["power-plan-saver"] = RecommendedAction.None,
        ["disable-autoplay"] = RecommendedAction.Enable,
        ["disable-sysmain"] = RecommendedAction.Disable,
        ["disable-wsearch"] = RecommendedAction.Disable,
        ["disable-hibernate"] = RecommendedAction.Enable,
        ["disable-background-apps"] = RecommendedAction.Enable,
        ["disable-visual-effects"] = RecommendedAction.Disable,
        ["disable-prefetch"] = RecommendedAction.None,
        ["enable-long-paths"] = RecommendedAction.Enable,
        ["disable-notification-tips"] = RecommendedAction.None,
        ["disable-xbox-services"] = RecommendedAction.None,
    };
}
