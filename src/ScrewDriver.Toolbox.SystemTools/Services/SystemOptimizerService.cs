using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Interfaces;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

[SupportedOSPlatform("windows")]
public class SystemOptimizerService : ISystemOptimizerService
{
    private static List<SystemSettingItem>? _settings;

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

    private static List<SettingDef>? _definitions;
    private static List<SettingDef> Definitions => _definitions ??= BuildDefinitions();

    private static List<SettingDef> BuildDefinitions()
    {
        return SystemUIDefinitions.Definitions
            .Concat(SecurityDefinitions.Definitions)
            .Concat(ExplorerDefinitions.Definitions)
            .Concat(AppearanceDefinitions.Definitions)
            .Concat(TaskbarDefinitions.Definitions)
            .Concat(PrivacyDefinitions.Definitions)
            .Concat(PerformanceDefinitions.Definitions)
            .ToList();
    }

    private static Dictionary<string, RecommendedAction>? _recommendations;
    private static Dictionary<string, RecommendedAction> RecommendationMap => _recommendations ??= BuildRecommendations();

    private static Dictionary<string, RecommendedAction> BuildRecommendations()
    {
        var all = new Dictionary<string, RecommendedAction>();
        foreach (var kv in SystemUIDefinitions.Recommendations) all[kv.Key] = kv.Value;
        foreach (var kv in SecurityDefinitions.Recommendations) all[kv.Key] = kv.Value;
        foreach (var kv in ExplorerDefinitions.Recommendations) all[kv.Key] = kv.Value;
        foreach (var kv in AppearanceDefinitions.Recommendations) all[kv.Key] = kv.Value;
        foreach (var kv in TaskbarDefinitions.Recommendations) all[kv.Key] = kv.Value;
        foreach (var kv in PrivacyDefinitions.Recommendations) all[kv.Key] = kv.Value;
        foreach (var kv in PerformanceDefinitions.Recommendations) all[kv.Key] = kv.Value;
        return all;
    }

    public List<SystemSettingItem> GetAllSettings()
    {
        if (_settings != null) return _settings;

        _settings = new List<SystemSettingItem>();
        foreach (var def in Definitions)
        {
            bool isEnabled = ReadCurrentState(def.Hive, def.KeyPath, def.ValueName, def.EnabledVal, def.Kind);

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
            var oldValue = ReadCurrentState(def.Hive, def.KeyPath, def.ValueName, def.EnabledVal, def.Kind);
            if (oldValue == enable)
            {
                Logger.Info($"Setting unchanged: {id} -> {(enable ? "enabled" : "disabled")} (already)");
                return true;
            }

            BackupManager.RecordSnapshot(id, $"{def.Hive}\\{def.KeyPath}", def.ValueName, oldValue, enable);

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

            SyncSettingsItem(id, enable);
            Logger.Info($"Setting applied: {id} -> {(enable ? "enabled" : "disabled")}");
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
            else if (id == "classic-context")
            {
                using var rootKey = RegistryKey.OpenBaseKey(def.Hive, RegistryView.Default);
                rootKey.DeleteSubKeyTree(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", throwOnMissingSubKey: false);
            }
            else
            {
                using var rootKey = RegistryKey.OpenBaseKey(def.Hive, RegistryView.Default);
                using var subKey = rootKey.OpenSubKey(def.KeyPath, writable: true);
                if (subKey != null)
                    subKey.DeleteValue(def.ValueName, throwOnMissingValue: false);
            }

            SyncSettingsItem(id, false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void SyncSettingsItem(string id, bool value)
    {
        var item = _settings?.Find(s => s.Id == id);
        if (item != null) item.IsEnabledSilent(value);
    }

    public async Task UninstallBloatwareAsync(IProgress<(string status, int progress)>? progress = null, string[]? selectedPackages = null)
    {
        var packages = selectedPackages ?? BloatwarePackages.Select(p => p.Name).ToArray();

        var total = packages.Length;
        var successCount = 0;
        var failCount = 0;

        for (var i = 0; i < total; i++)
        {
            var pkg = packages[i];
            try
            {
                // 1. Remove current user package
                RunPowerShell($"Get-AppxPackage *{pkg}* | Remove-AppxPackage", false);

                // 2. Remove provisioned package (prevents auto-install for new users)
                RunPowerShell($"Get-AppxProvisionedPackage -Online | Where-Object {{ /usr/bin/bash.DisplayName -like '*{pkg}*' }} | Remove-AppxProvisionedPackage -Online", false);

                successCount++;
            }
            catch
            {
                failCount++;
            }

            progress?.Report(($"正在卸载 {pkg} ({i + 1}/{total})", (i + 1) * 100 / total));
            await Task.Delay(80);
        }

        progress?.Report(($"卸载完成：成功 {successCount}，失败 {failCount}", 100));
    }
    public async Task<List<string>> CheckInstalledBloatwareAsync(string[] packageNames)
    {
        var installed = new List<string>();
        foreach (var pkg in packageNames)
        {
            try
            {
                var ps = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -Command \"Get-AppxPackage *{pkg}* | Select-Object -ExpandProperty Name\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                ps.Start();
                var output = await ps.StandardOutput.ReadToEndAsync();
                ps.WaitForExit();
                if (!string.IsNullOrWhiteSpace(output))
                    installed.Add(pkg);
            }
            catch { }
        }
        return installed;
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
                RegistryValueKind.String => string.Equals(current?.ToString(), enabledVal?.ToString(), StringComparison.OrdinalIgnoreCase),
                _ => Equals(current, enabledVal)
            };
        }
        catch
        {
            return false;
        }
    }

    public List<PresetDefinition> GetPresetDefinitions()
    {
        var all = GetAllSettings();

        // 新机推荐：所有 RecommendedAction.Enable 的项设为启用
        var newPcTargets = new Dictionary<string, bool>();
        foreach (var item in all)
        {
            if (item.Recommendation == RecommendedAction.Enable)
                newPcTargets[item.Id] = true;
        }

        // 极简模式：禁用干扰项
        var minimalTargets = new Dictionary<string, bool>
        {
            ["disable-widgets"] = true,
            ["disable-copilot"] = true,
            ["disable-search-highlights"] = true,
            ["disable-tips"] = true,
            ["disable-lockscreen"] = true,
            ["disable-transparency"] = true,
            ["disable-animation"] = true,
            ["disable-suggestions-in-start"] = true,
            ["start-hide-recent-items"] = true,
            ["start-hide-recently-added"] = true,
            ["hide-taskbar-search"] = true,
            ["hide-taskview"] = true,
            ["disable-cortana"] = true,
            ["taskbar-badge"] = false,
            ["ad-id"] = true,
            ["telemetry"] = true,
            ["disable-tailored-experiences"] = true,
            ["disable-cloud-search"] = true,
            ["disable-web-search"] = true,
            ["disable-activity-feed"] = true,
        };

        // 老电脑优化：关闭视觉效果和后台服务，启用游戏模式和高性能电源
        var oldPcTargets = new Dictionary<string, bool>
        {
            ["disable-visual-effects"] = true,
            ["disable-sysmain"] = true,
            ["disable-wsearch"] = true,
            ["disable-hibernate"] = true,
            ["disable-background-apps"] = true,
            ["disable-transparency"] = true,
            ["disable-animation"] = true,
            ["disable-xbox-services"] = true,
            ["disable-prefetch"] = true,
            ["disable-notification-tips"] = true,
            ["disable-thumbnails"] = true,
            ["disable-thumbnail-cache"] = true,
            ["game-mode"] = true,
            ["power-plan-high"] = true,
            ["fast-menu"] = true,
            ["vbs"] = true, // disable VBS for more performance
        };

        return new List<PresetDefinition>
        {
            new()
            {
                Id = "new-pc",
                Name = "新机推荐",
                Description = "一键启用所有推荐的安全优化项，适合新电脑或重装系统后使用",
                IconCode = "",
                TargetStates = newPcTargets
            },
            new()
            {
                Id = "minimal",
                Name = "极简模式",
                Description = "关闭小组件、搜索亮点、锁屏、动画、开始菜单推荐等干扰项，打造清爽体验",
                IconCode = "",
                TargetStates = minimalTargets
            },
            new()
            {
                Id = "old-pc",
                Name = "老电脑优化",
                Description = "关闭视觉效果和后台服务，启用游戏模式和高性能电源，最大化老电脑性能",
                IconCode = "",
                TargetStates = oldPcTargets
            }
        };
    }

    public void RestoreCategory(IEnumerable<string> ids)
    {
        foreach (var id in ids)
            RevertSetting(id);
    }
}
