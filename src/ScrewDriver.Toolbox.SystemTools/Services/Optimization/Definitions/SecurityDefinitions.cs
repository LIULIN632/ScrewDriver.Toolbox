using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class SecurityDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        ("noauto-update", "暂停 Windows Update", "禁用 Windows 自动更新推送，暂停系统更新",
         "安全与更新", RiskLevel.Dangerous, "暂停更新可能导致系统无法及时获取安全补丁", "删除 NoAutoUpdate 键值即可恢复自动更新",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("update-fully-disable", "完全禁用 Windows Update", "停止并禁用 Windows Update 相关服务（wuauserv/bits/dosvc）",
         "安全与更新", RiskLevel.Dangerous, "⚠️ 彻底关闭更新服务：安全更新缺失会导致系统漏洞，且 Microsoft Store 可能无法正常下载应用", "执行 DisablePsCommand 恢复服务启动类型",
         RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\wuauserv", "Start", 4, 2, RegistryValueKind.DWord,
         "PowerShell",
         "Stop-Service wuauserv,bits,dosvc -Force; Set-Service wuauserv -StartupType Disabled; Set-Service bits -StartupType Disabled; Set-Service dosvc -StartupType Disabled",
         "Set-Service wuauserv -StartupType Manual; Set-Service bits -StartupType Manual; Set-Service dosvc -StartupType Manual"),

        ("disable-defender", "关闭 Defender 实时保护", "禁用 Windows Defender 实时扫描保护",
         "安全与更新", RiskLevel.Dangerous, "关闭实时保护会使系统暴露在恶意软件威胁下，请确保有其他安全软件替代", "删除 DisableRealtimeMonitoring 键值即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("defender-cloud-protection", "关闭 Defender 云保护", "禁用 MAPS 云保护和样本自动提交",
         "安全与更新", RiskLevel.Dangerous, "关闭云保护会降低对新威胁的响应速度", "将 SpynetReporting 值改回 2 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Spynet", "SpynetReporting", 0, 2, RegistryValueKind.DWord,
         "Registry", null, null),

        ("defender-tamper-protection", "关闭 Defender 篡改保护", "⚠️ 仅供参考 — 篡改保护禁止外部程序修改注册表，仅可通过 Windows 安全中心手动操作",
         "安全与更新", RiskLevel.Dangerous, "此键受系统保护，修改注册表会被篡改保护拦截（循环依赖）。请在「Windows 安全中心 → 病毒和威胁防护 → 管理设置」中手动关闭", "在安全中心手动将篡改保护开关重新打开即可",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", 0, 5, RegistryValueKind.DWord,
         "Registry", null, null),

        ("defender-exclusion-path", "添加 Defender 排除路径", "将常用软件目录添加到 Defender 扫描排除列表",
         "安全与更新", RiskLevel.Optional, "排除路径中的文件将不会被扫描，请确保路径安全", "执行 DisablePsCommand 移除排除路径",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "Add-MpPreference -ExclusionPath 'C:\\Program Files'; Add-MpPreference -ExclusionPath 'C:\\Program Files (x86)'; Add-MpPreference -ExclusionProcess 'devenv.exe'; Add-MpPreference -ExclusionProcess 'msbuild.exe'",
         "Remove-MpPreference -ExclusionPath 'C:\\Program Files'; Remove-MpPreference -ExclusionPath 'C:\\Program Files (x86)'; Remove-MpPreference -ExclusionProcess 'devenv.exe'; Remove-MpPreference -ExclusionProcess 'msbuild.exe'"),

        ("disable-smartscreen", "关闭 SmartScreen", "禁用 Microsoft Defender SmartScreen 筛选器",
         "安全与更新", RiskLevel.Dangerous, "关闭后浏览器和系统不再检查下载文件与应用的安全性", "将 SmartScreenEnabled 改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableSmartScreen", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-telemetry", "深度禁用遥测", "将诊断数据设为 0(安全) 并禁用 DiagTrack 服务",
         "安全与更新", RiskLevel.Optional, "关闭后部分 Windows 预览体验功能可能受限，但隐私保护更彻底", "将 AllowTelemetry 改回 3 并重新启用 DiagTrack 服务",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, 3, RegistryValueKind.DWord,
         "PowerShell",
         "Set-ItemProperty 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection' -Name AllowTelemetry -Value 0; Stop-Service DiagTrack -Force; Set-Service DiagTrack -StartupType Disabled",
         "Set-ItemProperty 'HKLM:\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection' -Name AllowTelemetry -Value 3; Set-Service DiagTrack -StartupType Manual; Start-Service DiagTrack"),

        RegToggle("disable-pua-protection", "关闭 PUA 保护", "禁用 Windows Defender 潜在有害应用(PUA)检测",
            "安全与更新", RiskLevel.Dangerous, "关闭后 Defender 不再拦截广告软件/捆绑程序等低信誉应用", "将 PUAProtection 改回 1 即可恢复",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender", "PUAProtection", 0, 1),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["noauto-update"] = RecommendedAction.Enable,
        ["update-fully-disable"] = RecommendedAction.Disable,
        ["disable-defender"] = RecommendedAction.Disable,
        ["defender-cloud-protection"] = RecommendedAction.Disable,
        ["defender-tamper-protection"] = RecommendedAction.Disable,
        ["defender-exclusion-path"] = RecommendedAction.None,
        ["disable-smartscreen"] = RecommendedAction.Disable,
        ["disable-telemetry"] = RecommendedAction.Enable,
        ["disable-pua-protection"] = RecommendedAction.Disable,
    };
}
