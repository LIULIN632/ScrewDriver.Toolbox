using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Common;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public enum SoftwareControlType { Toggle, ActionButton }

public class SoftwareOptimizeItem : INotifyPropertyChanged
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public SoftwareControlType ControlType { get; set; } = SoftwareControlType.Toggle;
    public string ActionButtonText { get; set; } = "执行";
    public string OperationType { get; set; } = "Registry";
    public bool IsHklm { get; set; }
    public string? RegistrySubKey { get; set; }
    public string? RegistryValueName { get; set; }
    public int EnabledValue { get; set; } = 1;
    public int DisabledValue { get; set; } = 0;

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    public string StatusText => ControlType == SoftwareControlType.ActionButton
        ? (IsEnabled ? "可操作" : "不可用")
        : (IsEnabled ? "已优化" : "未优化");
    public string StatusColor => IsEnabled ? "#22C55E" : "#999999";

    public bool IsToggle => ControlType == SoftwareControlType.Toggle;
    public bool IsAction => ControlType == SoftwareControlType.ActionButton;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class SoftwareOptimizeViewModel : BaseViewModel
{
    private List<SoftwareOptimizeItem> _allItems = new();
    private string _selectedSoftware = "Edge";

    public ObservableCollection<SoftwareOptimizeItem> Items { get; } = new();
    public List<string> SoftwareList { get; } = new() { "Edge", "Office", "WPS" };

    public string SelectedSoftware
    {
        get => _selectedSoftware;
        set { if (SetProperty(ref _selectedSoftware, value)) FilterItems(); }
    }

    public RelayCommand<SoftwareOptimizeItem> ApplyActionCommand { get; }

    public SoftwareOptimizeViewModel()
    {
        ApplyActionCommand = new RelayCommand<SoftwareOptimizeItem>(ApplyAction, _ => true);
        LoadItems();
        FilterItems();
    }

    private void LoadItems()
    {
        _allItems = new List<SoftwareOptimizeItem>();
        _allItems.AddRange(BuildEdgeItems());
        _allItems.AddRange(BuildOfficeItems());
        _allItems.AddRange(BuildWpsItems());

        foreach (var item in _allItems.Where(i => i.ControlType == SoftwareControlType.Toggle))
            item.PropertyChanged += OnItemPropertyChanged;
    }

    // ── Edge items (10) — HKLM policy registry ──
    private static List<SoftwareOptimizeItem> BuildEdgeItems()
    {
        const string edgePol = @"SOFTWARE\Policies\Microsoft\Edge";
        const string edgeUpd = @"SOFTWARE\Policies\Microsoft\EdgeUpdate";
        return new List<SoftwareOptimizeItem>
        {
            NewToggle("edge-disable-startup-boost",   "关闭启动增强",     "禁止开机后台预加载 Edge 进程，释放内存占用",                     "Edge", edgePol, "StartupBoostEnabled",       0, 1),
            NewToggle("edge-disable-tab-preload",      "关闭标签页预加载",  "禁止预测性预加载网页，减少后台带宽与资源占用",                    "Edge", edgePol, "AllowTabPreloading",        0, 1),
            NewToggle("edge-disable-background-mode",  "禁用后台常驻",      "关闭 Edge 后彻底结束所有进程，不保留后台驻留",                    "Edge", edgePol, "BackgroundModeEnabled",     0, 1),
            NewToggle("edge-disable-auto-update",      "关闭自动更新",      "禁用 Edge 自动更新服务与升级推送",                               "Edge", edgeUpd, "UpdateDefault",             0, 2),
            NewToggle("edge-disable-sidebar",           "关闭右侧边栏",     "隐藏 Edge 右侧工具侧边栏（含 Copilot、集锦、购物等）",          "Edge", edgePol, "HubsSidebarEnabled",        0, 1),
            NewToggle("edge-disable-copilot",           "禁用 Copilot",    "彻底关闭 Edge 内置 Copilot AI 功能与入口",                       "Edge", edgePol, "CopilotEnabled",            0, 1),
            NewToggle("edge-disable-newtab-content",    "关闭新标签页资讯", "新标签页仅保留搜索框，移除资讯、广告、推荐内容",                 "Edge", edgePol, "NewTabPageContentEnabled",  0, 1),
            NewToggle("edge-disable-autoplay",          "禁用媒体自动播放", "禁止网页音频、视频自动播放，减少打扰与带宽消耗",                 "Edge", edgePol, "AutoplayAllowed",           0, 1),
            NewToggle("edge-enable-sleeping-tabs",      "开启标签页休眠",   "闲置标签自动休眠，释放内存与 CPU 资源",                          "Edge", edgePol, "SleepingTabsEnabled",       1, 0),
            NewToggle("edge-disable-shopping",          "关闭购物助手",     "禁用比价、优惠券等购物推荐功能，减少弹窗打扰",                   "Edge", edgePol, "ShoppingAssistantEnabled",  0, 1),
        };
    }

    private static SoftwareOptimizeItem NewToggle(string id, string name, string desc,
        string cat, string subKey, string valName, int onVal, int offVal)
    {
        var current = RegistryOptimizer.ReadDwordHKLM(subKey, valName, offVal);
        return new SoftwareOptimizeItem
        {
            Id = id, Name = name, Description = desc, Category = cat,
            ControlType = SoftwareControlType.Toggle,
            IsHklm = true, RegistrySubKey = subKey, RegistryValueName = valName,
            EnabledValue = onVal, DisabledValue = offVal,
            IsEnabled = current == onVal,
        };
    }

    // ── Office items (5) — 3 toggle + 2 action ──
    private static List<SoftwareOptimizeItem> BuildOfficeItems()
    {
        const string common = @"SOFTWARE\Microsoft\Office\16.0\Common";
        const string update = @"SOFTWARE\Microsoft\Office\16.0\Common\OfficeUpdate";
        const string general = @"SOFTWARE\Microsoft\Office\16.0\Common\General";
        var items = new List<SoftwareOptimizeItem>
        {
            NewToggle("office-disable-telemetry",     "关闭遥测上报", "禁用 Office 客户端诊断数据上报微软",
                "Office", common, "SendTelemetry", 0, 1),
            NewToggle("office-disable-auto-update",    "关闭自动更新", "禁用 Office 即点即用自动更新通道",
                "Office", update, "EnableAutomaticUpdates", 0, 1),
            NewToggle("office-disable-start-screen",   "关闭启动屏",   "跳过启动欢迎界面，直接打开空白文档",
                "Office", general, "DisableBootToOfficeStart", 1, 0),
            new SoftwareOptimizeItem
            {
                Id = "office-uninstall-plus", Name = "卸载 OfficePlus", Category = "Office",
                Description = "移除微软预装的 Office Plus 增值广告插件",
                ControlType = SoftwareControlType.ActionButton,
                ActionButtonText = "一键卸载",
                OperationType = "PowerShell",
                IsEnabled = IsOfficePlusInstalled(),
            },
            new SoftwareOptimizeItem
            {
                Id = "office-disable-com-addins", Name = "禁用第三方 COM 加载项", Category = "Office",
                Description = "禁用非微软官方的 COM 加载项，解决卡顿/崩溃问题",
                ControlType = SoftwareControlType.ActionButton,
                ActionButtonText = "一键禁用",
                OperationType = "Registry",
                IsEnabled = true,
            },
        };
        return items;
    }

    // ── WPS items (5, unchanged) ──
    private static List<SoftwareOptimizeItem> BuildWpsItems()
    {
        const string baseKey = @"SOFTWARE\Kingsoft\Office\6.0\Common";
        return new List<SoftwareOptimizeItem>
        {
            NewToggle("wps-disable-news",          "关闭资讯推送",   "关闭 WPS 热点资讯和模板推荐推送",   "WPS", baseKey, "NewsPop",          0, 1),
            NewToggle("wps-disable-auto-update",    "关闭自动更新",   "禁止 WPS 自动下载和安装更新",       "WPS", baseKey, "AutoUpdate",        0, 1),
            NewToggle("wps-disable-cloud-backup",   "关闭云备份提示", "不再弹窗提示备份文档到 WPS 云",      "WPS", baseKey, "CloudBackupPrompt", 0, 1),
            NewToggle("wps-disable-ad",             "关闭广告推送",   "停止 WPS 内置广告和产品推荐",       "WPS", baseKey, "AdPush",            0, 1),
            NewToggle("wps-disable-home-tab",       "关闭首页标签广告","隐藏 WPS 首页信息流和推广内容",     "WPS", baseKey, "HomeTabAds",        0, 1),
        };
    }

    // ── Action dispatch ──
    private async void ApplyAction(SoftwareOptimizeItem? item)
    {
        if (item == null) return;
        if (!SettingApplyGuard.TryEnter()) return;
        IsBusy = true;
        try
        {
            switch (item.Id)
            {
                case "office-uninstall-plus":
                    await Task.Run(UninstallOfficePlus);
                    item.IsEnabled = IsOfficePlusInstalled();
                    MessageBox.Show(item.IsEnabled
                        ? "OfficePlus 卸载可能未完成，请检查控制面板。"
                        : "OfficePlus 已成功卸载。", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "office-disable-com-addins":
                    var count = await Task.Run(DisableThirdPartyComAddins);
                    MessageBox.Show(count > 0
                        ? $"已禁用 {count} 个第三方 COM 加载项。\n请重启 Office 软件生效。"
                        : "未发现需要禁用的第三方 COM 加载项。", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
        finally
        {
            IsBusy = false;
            SettingApplyGuard.Exit();
        }
    }

    // ── Toggle change handler ──
    private async void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SoftwareOptimizeItem.IsEnabled)) return;
        if (sender is not SoftwareOptimizeItem item) return;
        if (!SettingApplyGuard.TryEnter()) return;

        IsBusy = true;
        try
        {
            var ok = await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(item.RegistrySubKey) || string.IsNullOrEmpty(item.RegistryValueName))
                    return false;
                var value = item.IsEnabled ? item.EnabledValue : item.DisabledValue;
                return item.IsHklm
                    ? RegistryOptimizer.WriteDwordHKLM(item.RegistrySubKey, item.RegistryValueName, value)
                    : RegistryOptimizer.WriteDword(item.RegistrySubKey, item.RegistryValueName, value);
            });

            if (!ok)
            {
                item.IsEnabled = !item.IsEnabled;
                MessageBox.Show($"「{item.Name}」设置失败，请以管理员身份运行。", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsBusy = false;
            SettingApplyGuard.Exit();
        }
    }

    private void FilterItems()
    {
        Items.Clear();
        foreach (var item in _allItems)
            if (item.Category == SelectedSoftware)
                Items.Add(item);
    }

    // ── OfficePlus detection & uninstall ──
    private static bool IsOfficePlusInstalled()
    {
        try
        {
            string[] uninstallPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            foreach (var basePath in uninstallPaths)
            {
                using var root = Registry.LocalMachine.OpenSubKey(basePath);
                if (root == null) continue;
                foreach (var subKeyName in root.GetSubKeyNames())
                {
                    using var subKey = root.OpenSubKey(subKeyName);
                    var displayName = subKey?.GetValue("DisplayName")?.ToString();
                    if (displayName != null && displayName.Contains("OfficePlus"))
                        return true;
                }
            }
            return false;
        }
        catch { return false; }
    }

    private static string? GetOfficePlusUninstallString()
    {
        try
        {
            string[] uninstallPaths =
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            foreach (var basePath in uninstallPaths)
            {
                using var root = Registry.LocalMachine.OpenSubKey(basePath);
                if (root == null) continue;
                foreach (var subKeyName in root.GetSubKeyNames())
                {
                    using var subKey = root.OpenSubKey(subKeyName);
                    var displayName = subKey?.GetValue("DisplayName")?.ToString();
                    if (displayName != null && displayName.Contains("OfficePlus"))
                        return subKey?.GetValue("UninstallString")?.ToString();
                }
            }
            return null;
        }
        catch { return null; }
    }

    private static bool UninstallOfficePlus()
    {
        try
        {
            var uninstallString = GetOfficePlusUninstallString();
            if (string.IsNullOrEmpty(uninstallString)) return false;

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {uninstallString} /quiet /norestart",
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            var proc = Process.Start(psi);
            proc?.WaitForExit(30000);
            return !IsOfficePlusInstalled();
        }
        catch { return false; }
    }

    // ── COM add-in disabler ──
    private static int DisableThirdPartyComAddins()
    {
        var disabledCount = 0;
        string[] components = { "Word", "Excel", "PowerPoint" };

        foreach (var component in components)
        {
            var addinsPath = $@"Software\Microsoft\Office\{component}\Addins";
            using var addinsKey = Registry.CurrentUser.OpenSubKey(addinsPath, writable: true);
            if (addinsKey == null) continue;

            foreach (var progId in addinsKey.GetSubKeyNames())
            {
                if (progId.StartsWith("Microsoft.") || progId.StartsWith("Office."))
                    continue;

                using var itemKey = addinsKey.OpenSubKey(progId, writable: true);
                if (itemKey == null) continue;

                var loadBehavior = itemKey.GetValue("LoadBehavior");
                if (loadBehavior is not int lb || lb != 3) continue;

                BackupManager.RecordSnapshot(
                    $"com-addin-{component}-{progId}",
                    $@"HKCU\{addinsPath}\{progId}",
                    "LoadBehavior",
                    loadBehavior, false);

                itemKey.SetValue("LoadBehavior", 2, RegistryValueKind.DWord);
                disabledCount++;
            }
        }
        return disabledCount;
    }
}
