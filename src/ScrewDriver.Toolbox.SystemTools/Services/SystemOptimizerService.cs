using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Interfaces;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.SystemTools.Services;

[SupportedOSPlatform("windows")]
public class SystemOptimizerService : ISystemOptimizerService
{
    private List<SystemSettingItem>? _settings;

    public static readonly (string Name, string Description)[] BloatwarePackages =
    {
        ("Microsoft.BingNews", "Bing 新闻"),
        ("Microsoft.BingWeather", "Bing 天气"),
        ("Microsoft.GetHelp", "获取帮助"),
        ("Microsoft.Getstarted", "提示"),
        ("Microsoft.Microsoft3DViewer", "3D 查看器"),
        ("Microsoft.MicrosoftOfficeHub", "Office Hub"),
        ("Microsoft.MicrosoftSolitaireCollection", "微软纸牌合集"),
        ("Microsoft.MixedReality.Portal", "混合现实门户"),
        ("Microsoft.Office.OneNote", "OneNote"),
        ("Microsoft.People", "人脉"),
        ("Microsoft.SkypeApp", "Skype"),
        ("Microsoft.Todos", "Microsoft To Do"),
        ("Microsoft.Wallet", "电子钱包"),
        ("Microsoft.WindowsAlarms", "闹钟和时钟"),
        ("Microsoft.WindowsCamera", "相机"),
        ("Microsoft.WindowsCommunicationsApps", "邮件和日历"),
        ("Microsoft.WindowsFeedbackHub", "反馈中心"),
        ("Microsoft.WindowsMaps", "地图"),
        ("Microsoft.WindowsSoundRecorder", "录音机"),
        ("Microsoft.XboxApp", "Xbox 应用"),
        ("Microsoft.XboxGameCallableUI", "Xbox Game UI"),
        ("Microsoft.XboxSpeechToTextOverlay", "Xbox 语音"),
        ("Microsoft.YourPhone", "手机连接"),
        ("Microsoft.ZuneMusic", "Groove 音乐"),
        ("Microsoft.ZuneVideo", "电影和电视"),
        ("Microsoft.OneConnect", "One Connect"),
        ("Microsoft.MSPaint", "画图"),
        ("Microsoft.Whiteboard", "白板"),
        ("Microsoft.Windows.Photos", "照片"),
        ("Clipchamp.Clipchamp", "Clipchamp 视频编辑器"),
    };

    private static readonly List<(string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
        RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal, RegistryValueKind Kind,
        string OperationType, string? EnablePsCmd, string? DisablePsCmd)> Definitions = new()
    {
        // ================================================================
        // Windows 更新
        // ================================================================
        ("noauto-update", "暂停 Windows Update", "禁用 Windows 自动更新推送，暂停系统更新",
         "Windows 更新", RiskLevel.Dangerous, "暂停更新可能导致系统无法及时获取安全补丁", "删除 NoAutoUpdate 键值即可恢复自动更新",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("update-fully-disable", "完全禁用 Windows Update", "停止并禁用 Windows Update 相关服务（wuauserv/bits/dosvc）",
         "Windows 更新", RiskLevel.Dangerous, "彻底关闭更新服务，系统将无法获取任何更新", "执行 DisablePsCommand 恢复服务启动类型",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "Stop-Service wuauserv,bits,dosvc -Force; Set-Service wuauserv -StartupType Disabled; Set-Service bits -StartupType Disabled; Set-Service dosvc -StartupType Disabled",
         "Set-Service wuauserv -StartupType Manual; Set-Service bits -StartupType Manual; Set-Service dosvc -StartupType Manual"),

        // ================================================================
        // Defender 完整管理
        // ================================================================
        ("disable-defender", "关闭 Defender 实时保护", "禁用 Windows Defender 实时扫描保护",
         "Defender", RiskLevel.Dangerous, "关闭实时保护会使系统暴露在恶意软件威胁下，请确保有其他安全软件替代", "删除 DisableRealtimeMonitoring 键值即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableRealtimeMonitoring", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("defender-cloud-protection", "关闭 Defender 云保护", "禁用 MAPS 云保护和样本自动提交",
         "Defender", RiskLevel.Dangerous, "关闭云保护会降低对新威胁的响应速度", "执行 DisablePsCommand 恢复云保护级别",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "Set-MpPreference -MAPSReporting Disabled -SubmitSamplesConsent NeverSend",
         "Set-MpPreference -MAPSReporting Advanced -SubmitSamplesConsent AlwaysPrompt"),

        ("defender-tamper-protection", "关闭 Defender 篡改保护", "允许其他安全软件修改 Defender 设置",
         "Defender", RiskLevel.Dangerous, "关闭篡改保护后恶意软件可能修改 Defender 配置", "将 TamperProtection 值改回 5 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", 0, 5, RegistryValueKind.DWord,
         "Registry", null, null),

        ("defender-exclusion-path", "添加 Defender 排除路径", "将常用软件目录添加到 Defender 扫描排除列表",
         "Defender", RiskLevel.Optional, "排除路径中的文件将不会被扫描，请确保路径安全", "执行 DisablePsCommand 移除排除路径",
         RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
         "PowerShell",
         "Add-MpPreference -ExclusionPath 'C:\\Program Files'; Add-MpPreference -ExclusionPath 'C:\\Program Files (x86)'; Add-MpPreference -ExclusionProcess 'devenv.exe'; Add-MpPreference -ExclusionProcess 'msbuild.exe'",
         "Remove-MpPreference -ExclusionPath 'C:\\Program Files'; Remove-MpPreference -ExclusionPath 'C:\\Program Files (x86)'; Remove-MpPreference -ExclusionProcess 'devenv.exe'; Remove-MpPreference -ExclusionProcess 'msbuild.exe'"),

        // ================================================================
        // 隐私与遥测
        // ================================================================
        ("ad-id", "关闭广告 ID", "禁用 Windows 广告标识符，减少个性化广告追踪",
         "隐私与遥测", RiskLevel.Recommended, "关闭后部分应用可能无法提供个性化内容", "将 Enabled 值改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "Enabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("telemetry", "关闭遥测数据", "将 Windows 诊断数据收集级别设为最低（仅安全）",
         "隐私与遥测", RiskLevel.Recommended, "关闭遥测后部分 Windows 预览体验功能可能受限", "将 AllowTelemetry 改回 3 即可恢复默认级别",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, 3, RegistryValueKind.DWord,
         "Registry", null, null),

        ("activity-history", "关闭活动历史记录", "禁止 Windows 记录和上传活动历史到云端",
         "隐私与遥测", RiskLevel.Recommended, "关闭后无法跨设备同步活动历史和时间线", "将 EnableActivityFeed 改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-copilot", "禁用 Windows Copilot", "移除任务栏 Copilot 图标并禁用 AI 助手功能",
         "隐私与遥测", RiskLevel.Optional, "会完全禁用 Copilot 功能，无法使用 AI 助手", "删除 TurnOffWindowsCopilot 值即可恢复",
         RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-tips", "关闭 Windows 提示", "禁用设置首页和锁屏的提示与建议内容",
         "隐私与遥测", RiskLevel.Recommended, "仅关闭系统提示，不影响其他通知功能", "将 SoftLandingEnabled 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        // ================================================================
        // 资源管理器增强
        // ================================================================
        ("show-extensions", "显示文件扩展名", "在资源管理器中显示已知文件类型的扩展名",
         "资源管理器", RiskLevel.Recommended, "仅影响文件名的显示方式，不影响文件本身", "将 HideFileExt 值改回 1 即可隐藏扩展名",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-hidden-files", "显示隐藏文件", "在资源管理器中显示隐藏文件和文件夹",
         "资源管理器", RiskLevel.Optional, "隐藏文件通常包含系统配置，误删可能导致问题", "将 Hidden 值改回 2 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, 2, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-system-files", "显示系统文件", "取消隐藏受保护的操作系统文件",
         "资源管理器", RiskLevel.Dangerous, "系统文件删除后可能导致系统崩溃，请谨慎操作", "将 ShowSuperHidden 改回 0 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("expand-to-current-folder", "展开到当前文件夹", "导航窗格自动展开到当前打开的文件夹",
         "资源管理器", RiskLevel.Recommended, "仅改变导航窗格行为，不影响文件数据", "将 NavPaneExpandToCurrentFolder 改回 0 即可关闭",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "NavPaneExpandToCurrentFolder", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("classic-context", "经典右键菜单", "恢复 Windows 10 风格的完整右键菜单（跳过「显示更多选项」）",
         "资源管理器", RiskLevel.Optional, "修改注册表 CLSID 可能影响右键菜单稳定性", "删除该 CLSID 键即可恢复 Win11 默认菜单",
         RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", null, RegistryValueKind.String,
         "Registry", null, null),

        ("restore-photo-viewer", "恢复照片查看器", "恢复 Windows 经典照片查看器为默认图片浏览器",
         "资源管理器", RiskLevel.Optional, "照片查看器不支持部分新格式（如 HEIC/WebP）", "删除 PhotoViewer 注册表项即可还原",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows Photo Viewer\Capabilities\FileAssociations", ".jpg", "PhotoViewer.FileAssoc.Tiff", null, RegistryValueKind.String,
         "Registry", null, null),

        // ================================================================
        // 任务栏与开始菜单
        // ================================================================
        ("taskbar-combine", "任务栏不合并", "任务栏按钮不合并，始终显示标签",
         "任务栏", RiskLevel.Optional, "仅影响任务栏外观，可随时恢复", "将 TaskbarGlomLevel 改回 1 即可合并",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarGlomLevel", 2, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-widgets", "关闭小组件", "禁用任务栏小组件按钮和资讯推送",
         "任务栏", RiskLevel.Recommended, "关闭后无法使用天气、资讯等小组件功能", "将 AllowNewsAndInterests 改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-search-highlights", "关闭搜索亮点", "禁用任务栏搜索框的每日亮点图标和推荐",
         "任务栏", RiskLevel.Recommended, "仅关闭搜索框的图形亮点，不影响搜索功能", "将 IsDynamicSearchBoxEnabled 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        // ================================================================
        // 性能与电源
        // ================================================================
        ("game-mode", "游戏模式", "启用 Windows 游戏模式，优化游戏性能",
         "性能与电源", RiskLevel.Recommended, "游戏模式可能影响后台应用的运行", "将 AllowAutoGameMode 改回 0 即可关闭",
         RegistryHive.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("vbs", "关闭 VBS 内存完整性", "禁用基于虚拟化的安全（VBS）和内存完整性检查",
         "性能与电源", RiskLevel.Dangerous, "VBS 是重要安全特性，关闭后可能降低系统安全性", "将 EnableVirtualizationBasedSecurity 改回 1 即可重新启用",
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
    };

    private static readonly Dictionary<string, RecommendedAction> RecommendationMap = new()
    {
        ["noauto-update"] = RecommendedAction.Enable,
        ["update-fully-disable"] = RecommendedAction.Disable,
        ["disable-defender"] = RecommendedAction.Disable,
        ["defender-cloud-protection"] = RecommendedAction.Disable,
        ["defender-tamper-protection"] = RecommendedAction.Disable,
        ["defender-exclusion-path"] = RecommendedAction.None,
        ["ad-id"] = RecommendedAction.Enable,
        ["telemetry"] = RecommendedAction.Enable,
        ["activity-history"] = RecommendedAction.Enable,
        ["disable-copilot"] = RecommendedAction.Enable,
        ["disable-tips"] = RecommendedAction.Enable,
        ["show-extensions"] = RecommendedAction.Enable,
        ["show-hidden-files"] = RecommendedAction.Enable,
        ["show-system-files"] = RecommendedAction.Disable,
        ["expand-to-current-folder"] = RecommendedAction.Enable,
        ["classic-context"] = RecommendedAction.Enable,
        ["restore-photo-viewer"] = RecommendedAction.Enable,
        ["taskbar-combine"] = RecommendedAction.Enable,
        ["disable-widgets"] = RecommendedAction.Enable,
        ["disable-search-highlights"] = RecommendedAction.Enable,
        ["game-mode"] = RecommendedAction.Enable,
        ["vbs"] = RecommendedAction.Enable,
        ["power-plan-high"] = RecommendedAction.None,
        ["power-plan-balanced"] = RecommendedAction.None,
        ["power-plan-saver"] = RecommendedAction.None,
    };

    public List<SystemSettingItem> GetAllSettings()
    {
        if (_settings != null) return _settings;

        _settings = new List<SystemSettingItem>();
        foreach (var def in Definitions)
        {
            bool isEnabled;
            if (def.OperationType == "PowerShell")
                isEnabled = false;
            else
                isEnabled = ReadCurrentState(def.Hive, def.KeyPath, def.ValueName, def.EnabledVal, def.Kind);

            RecommendationMap.TryGetValue(def.Id, out var rec);

            _settings.Add(new SystemSettingItem
            {
                Id = def.Id,
                Name = def.Name,
                Description = def.Desc,
                Category = def.Cat,
                RiskLevel = def.Risk,
                RiskDescription = def.RiskDesc,
                Recommendation = rec,
                CanRevert = true,
                RevertMethodDescription = def.Revert,
                OperationType = def.OperationType,
                EnablePsCommand = def.EnablePsCmd,
                DisablePsCommand = def.DisablePsCmd,
                IsEnabled = isEnabled
            });
        }
        return _settings;
    }

    public bool ApplySetting(string id, bool enable)
    {
        var def = Definitions.Find(d => d.Id == id);
        if (def == default) return false;

        try
        {
            if (def.OperationType == "PowerShell")
            {
                var cmd = enable ? def.EnablePsCmd : def.DisablePsCmd;
                if (string.IsNullOrEmpty(cmd)) return false;
                RunPowerShell(cmd);
            }
            else
            {
                using var rootKey = RegistryKey.OpenBaseKey(def.Hive, RegistryView.Default);
                using var subKey = rootKey.CreateSubKey(def.KeyPath);
                if (subKey == null) return false;

                var value = enable ? def.EnabledVal : def.DisabledVal;
                if (value == null)
                    subKey.DeleteValue(def.ValueName, throwOnMissingValue: false);
                else
                    subKey.SetValue(def.ValueName, value, def.Kind);
            }

            var item = _settings?.Find(s => s.Id == id);
            if (item != null) item.IsEnabled = enable;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool RevertSetting(string id)
    {
        var def = Definitions.Find(d => d.Id == id);
        if (def == default) return false;

        try
        {
            if (def.OperationType == "PowerShell")
            {
                if (!string.IsNullOrEmpty(def.DisablePsCmd))
                    RunPowerShell(def.DisablePsCmd);
            }
            else
            {
                using var rootKey = RegistryKey.OpenBaseKey(def.Hive, RegistryView.Default);
                using var subKey = rootKey.OpenSubKey(def.KeyPath, writable: true);
                if (subKey != null)
                    subKey.DeleteValue(def.ValueName, throwOnMissingValue: false);
            }

            var item = _settings?.Find(s => s.Id == id);
            if (item != null) item.IsEnabled = false;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task UninstallBloatwareAsync(IProgress<(string status, int progress)>? progress = null, string[]? selectedPackages = null)
    {
        var packages = selectedPackages ?? BloatwarePackages.Select(p => p.Name).ToArray();

        var total = packages.Length;
        for (var i = 0; i < total; i++)
        {
            var pkg = packages[i];
            try
            {
                RunPowerShell($"Get-AppxPackage *{pkg}* | Remove-AppxPackage", false);
            }
            catch
            {
                // Continue even if individual package removal fails
            }

            progress?.Report(($"正在卸载 {pkg} ({i + 1}/{total})", (i + 1) * 100 / total));
            await Task.Delay(80);
        }

        progress?.Report(("预装应用卸载完成", 100));
    }

    public string? GetCurrentPowerPlan()
    {
        try
        {
            var psi = new ProcessStartInfo("powercfg", "/getactivescheme")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            if (proc == null) return null;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(3000);

            if (output.Contains("8c5e7fda")) return "power-plan-high";
            if (output.Contains("381b4222")) return "power-plan-balanced";
            if (output.Contains("a1841308")) return "power-plan-saver";
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static void RunPowerShell(string command, bool wait = true)
    {
        var psi = new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var proc = Process.Start(psi);
        if (wait)
            proc?.WaitForExit(10000);
    }

    private static bool ReadCurrentState(RegistryHive hive, string keyPath, string valueName, object enabledVal, RegistryValueKind kind)
    {
        try
        {
            using var rootKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var subKey = rootKey.OpenSubKey(keyPath);
            if (subKey == null) return false;

            var current = subKey.GetValue(valueName);
            if (current == null) return false;

            return kind switch
            {
                RegistryValueKind.DWord => Convert.ToInt32(current) == Convert.ToInt32(enabledVal),
                RegistryValueKind.String => string.IsNullOrEmpty(current.ToString()) == string.IsNullOrEmpty(enabledVal?.ToString()),
                _ => Equals(current, enabledVal)
            };
        }
        catch
        {
            return false;
        }
    }
}
