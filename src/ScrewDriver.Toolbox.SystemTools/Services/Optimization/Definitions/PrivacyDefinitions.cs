using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class PrivacyDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        ("ad-id", "关闭广告 ID", "禁用 Windows 广告标识符，减少个性化广告追踪",
         "隐私与搜索", RiskLevel.Recommended, "关闭后部分应用可能无法提供个性化内容", "将 Enabled 值改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "Enabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("telemetry", "关闭遥测数据", "将 Windows 诊断数据收集级别设为最低（仅安全）",
         "隐私与搜索", RiskLevel.Recommended, "关闭遥测后部分 Windows 预览体验功能可能受限", "将 AllowTelemetry 改回 3 即可恢复默认级别",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, 3, RegistryValueKind.DWord,
         "Registry", null, null),

        ("activity-history", "关闭活动历史记录", "禁止 Windows 记录和上传活动历史到云端",
         "隐私与搜索", RiskLevel.Recommended, "关闭后无法跨设备同步活动历史和时间线", "将 EnableActivityFeed 改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-copilot", "禁用 Windows Copilot", "移除任务栏 Copilot 图标并禁用 AI 助手功能",
         "隐私与搜索", RiskLevel.Optional, "会完全禁用 Copilot 功能，无法使用 AI 助手", "删除 TurnOffWindowsCopilot 值即可恢复",
         RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-tips", "关闭 Windows 提示", "禁用设置首页和锁屏的提示与建议内容",
         "隐私与搜索", RiskLevel.Recommended, "仅关闭系统提示，不影响其他通知功能", "将 SoftLandingEnabled 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-tailored-experiences", "关闭量身定制体验", "禁止 Microsoft 根据设备数据提供个性化建议",
         "隐私与搜索", RiskLevel.Recommended, "仅关闭个性化推荐，不影响系统更新", "将对应键值改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-find-my-device", "关闭查找我的设备", "禁止 Microsoft 定期记录设备位置信息",
         "隐私与搜索", RiskLevel.Recommended, "关闭后将无法使用「查找我的设备」功能定位电脑", "将对应键值改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Settings\FindMyDevice", "LocationSyncEnabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        RegToggle("disable-cloud-search", "关闭云搜索", "不在 Windows 搜索中包含 OneDrive/SharePoint 等云内容",
            "隐私与搜索", RiskLevel.Recommended, "仅影响搜索范围，不影响云存储功能", "将 AllowCloudSearch 改回 1 即可恢复",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0, 1),

        RegToggle("disable-web-search", "关闭 Web 搜索", "Windows 搜索不再包含 Bing 网络搜索结果",
            "隐私与搜索", RiskLevel.Recommended, "仅影响系统搜索框的 Web 结果，不影响浏览器搜索", "将 BingSearchEnabled 改回 1 即可恢复",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, 1),

        ("disable-activity-feed", "关闭活动源", "禁止 Windows 记录和同步跨设备活动历史",
            "隐私与搜索", RiskLevel.Recommended, "关闭后无法跨设备查看最近使用的文件和应用", "将 EnableActivityFeed 改回 1 即可恢复",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, 1, RegistryValueKind.DWord,
            "Registry", null, null),

        RegToggle("disable-error-reporting", "关闭 Windows 错误报告", "禁止系统在应用崩溃时自动向 Microsoft 发送错误报告",
            "隐私与搜索", RiskLevel.Recommended, "关闭后错误报告不会上传，但也不会获得 Microsoft 的问题解决方案", "将 Disabled 改回 0 即可恢复",
            RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, 0),

        RegToggle("disable-feedback-frequency", "关闭反馈请求", "禁止 Windows 定期弹出反馈频率调查",
            "隐私与搜索", RiskLevel.Recommended, "仅关闭反馈弹窗，不影响其他通知功能", "将 NumberOfSIUFInPeriod 改回 1 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, 1),

        RegToggle("disable-onedrive", "禁用 OneDrive 同步", "禁止 OneDrive 开机启动和文件同步（卸载 OneDrive 前建议先禁用）",
            "隐私与搜索", RiskLevel.Optional, "关闭后 OneDrive 文件不会同步，但本地文件不受影响", "将 DisableFileSyncNGSC 改回 0 即可",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", 1, 0),

        RegToggle("disable-app-diagnostics", "关闭应用诊断访问", "禁止应用访问和上传诊断信息",
            "隐私与搜索", RiskLevel.Recommended, "关闭后应用的诊断反馈功能可能受限", "将 LetAppsGetDiagnosticInfo 改回 1 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics", "Value", 0, 1),

        RegToggle("disable-location", "关闭位置服务", "禁止 Windows 和应用访问设备位置信息",
            "隐私与搜索", RiskLevel.Recommended, "关闭后地图、天气等依赖位置的应用可能无法提供本地化服务", "将 LocationSyncEnabled 和位置服务开关改回 1 即可",
            RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", 0, 1),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["ad-id"] = RecommendedAction.Enable,
        ["telemetry"] = RecommendedAction.Enable,
        ["activity-history"] = RecommendedAction.Enable,
        ["disable-copilot"] = RecommendedAction.Enable,
        ["disable-tips"] = RecommendedAction.Enable,
        ["disable-tailored-experiences"] = RecommendedAction.Enable,
        ["disable-find-my-device"] = RecommendedAction.Enable,
        ["disable-cloud-search"] = RecommendedAction.Enable,
        ["disable-web-search"] = RecommendedAction.Enable,
        ["disable-activity-feed"] = RecommendedAction.Enable,
        ["disable-error-reporting"] = RecommendedAction.Enable,
        ["disable-feedback-frequency"] = RecommendedAction.Enable,
        ["disable-onedrive"] = RecommendedAction.None,
        ["disable-app-diagnostics"] = RecommendedAction.Enable,
        ["disable-location"] = RecommendedAction.Enable,
    };
}
