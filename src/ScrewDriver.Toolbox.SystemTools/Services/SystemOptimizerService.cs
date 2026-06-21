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

    public List<PresetDefinition> GetDefaultPresetDefinitions()
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

        // 新机到手设置：基于小黑盒「新电脑新机到手必做设置」文章，20 项
        var newPcSetupTargets = new Dictionary<string, bool>
        {
            // 桌面图标
            ["show-this-pc"] = true,
            ["show-control-panel-desktop"] = true,
            // 隐私
            ["ad-id"] = true,
            ["disable-tailored-experiences"] = true,
            ["disable-tips"] = true,
            ["telemetry"] = true,
            // 任务栏
            ["disable-widgets"] = true,
            ["disable-search-highlights"] = true,
            ["disable-news-interests"] = true,
            // 开始菜单
            ["disable-suggestions-in-start"] = true,
            ["disable-cortana"] = true,
            // 资源管理器
            ["show-extensions"] = true,
            ["classic-context"] = true,
            ["expand-to-current-folder"] = true,
            ["show-full-path-in-title"] = true,
            ["open-to-pc"] = true,
            // 系统界面
            ["enable-numlock"] = true,
            ["disable-sticky-keys"] = true,
            // 性能与更新
            ["power-plan-high"] = true,
            ["noauto-update"] = true,
        };

        return new List<PresetDefinition>
        {
            new()
            {
                Id = "new-pc",
                Name = "新机推荐",
                Tag = "推荐 · 安全",
                Description = "一键启用所有推荐的安全优化项，关闭广告跟踪、遥测数据、活动历史，适合新电脑或重装系统后初始化使用",
                IconCode = "🛡️",
                Scene = "新装系统/重装系统后初始化，追求隐私安全无广告，不改动系统核心功能",
                Effect = "关闭广告跟踪、遥测收集、冗余弹窗，降低后台隐私泄露风险，无副作用",
                Notice = "仅修改系统界面和隐私配置，不影响系统更新、安全防护、硬件驱动，新手可放心使用",
                TargetStates = newPcTargets
            },
            new()
            {
                Id = "new-pc-setup",
                Name = "新机到手设置",
                Tag = "必做 · 全面",
                Description = "基于小黑盒新机攻略：桌面图标、隐私四项全关、任务栏精简、资源管理器增强、高性能电源、暂停更新，一步到位",
                IconCode = "🆕",
                Scene = "新电脑开箱 / 重装系统后第一步，覆盖桌面、隐私、资源管理器、电源、更新等核心配置",
                Effect = "桌面显示此电脑+控制面板，关闭广告跟踪与遥测，精简任务栏和开始菜单，开启高性能电源，暂停自动更新",
                Notice = "含暂停 Windows Update 操作，后续可手动恢复。Nvidia 显卡驱动设置和存储感知需手动操作",
                TargetStates = newPcSetupTargets
            },
            new()
            {
                Id = "minimal",
                Name = "极简模式",
                Tag = "界面精简",
                Description = "关闭小组件、搜索框、动画效果、任务视图，精简系统界面，降低资源占用，追求纯净桌面体验",
                IconCode = "🎯",
                Scene = "追求纯净桌面体验，不需要小组件、搜索框等冗余功能",
                Effect = "精简任务栏、开始菜单、资源管理器，减少UI动画资源占用，桌面更清爽",
                Notice = "所有界面修改均可一键恢复，不会破坏系统文件，部分设置需重启资源管理器生效",
                TargetStates = minimalTargets
            },
            new()
            {
                Id = "old-pc",
                Name = "老电脑优化",
                Tag = "性能优先",
                Description = "关闭动画效果、启用高性能电源计划、禁用 VBS，最大化释放硬件性能，适合低配置老旧设备",
                IconCode = "⚡",
                Scene = "低配置老旧电脑、机械硬盘设备，优先保证运行流畅度",
                Effect = "释放 10%-20% 内存和磁盘占用，提升开机和操作流畅度",
                Notice = "关闭 VBS 会降低系统安全性，但可显著提升游戏和日常使用性能。所有设置均可恢复",
                TargetStates = oldPcTargets
            }
        };
    }

    public List<PresetDefinition> GetPresetDefinitions()
    {
        return PresetStore.LoadPresets(GetDefaultPresetDefinitions);
    }

    public void RestoreCategory(IEnumerable<string> ids)
    {
        foreach (var id in ids)
            RevertSetting(id);
    }
}
