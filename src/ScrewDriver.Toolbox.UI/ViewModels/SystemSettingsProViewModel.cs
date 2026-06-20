using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.SystemTools.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SystemSettingsProViewModel : BaseViewModel
{
    private readonly SystemOptimizerService _service = new();
    private bool _suppressChanges;
    private string _selectedCategory = "全部";
    private string _searchText = string.Empty;
    private List<SystemSettingItem> _allSettings = new();

    public ObservableCollection<SettingGroup> Groups { get; } = new();

    public List<string> Categories { get; } = new()
    {
        "全部", "个性化", "桌面", "资源管理器", "任务栏",
        "开始菜单", "系统界面", "隐私与搜索", "性能与电源", "安全与更新"
    };

    public List<PresetDefinition> Presets { get; private set; } = new();

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { if (SetProperty(ref _selectedCategory, value)) FilterGroups(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) FilterGroups(); }
    }

    private static readonly Dictionary<string, string> IconMap = new()
    {
        // 资源管理器
        ["show-extensions"] = "📄", ["show-hidden-files"] = "👁", ["show-system-files"] = "⚙",
        ["expand-to-current-folder"] = "📂", ["classic-context"] = "🖱", ["restore-photo-viewer"] = "🖼",
        ["disable-quick-access-history"] = "🕐", ["show-full-path-in-title"] = "📍",
        ["show-status-bar"] = "📊", ["open-to-pc"] = "💻", ["hide-common-folders"] = "📁",
        ["nav-pane-all-folders"] = "🌲", ["show-libraries"] = "📚", ["compact-mode"] = "📏",
        ["check-box-select"] = "☑", ["show-drive-letters-before"] = "🔤",
        ["show-encrypted-compressed-color"] = "🎨", ["launch-folder-windows-separate"] = "🪟",
        ["hide-sync-provider"] = "☁", ["disable-thumbnails"] = "🖼", ["disable-thumbnail-cache"] = "💾",
        // 个性化
        ["disable-lockscreen"] = "🔒", ["show-this-pc"] = "🖥", ["show-recycle-bin"] = "🗑",
        ["disable-login-password"] = "🔑", ["disable-animation"] = "✨", ["disable-transparency"] = "🪟",
        ["dark-mode-apps"] = "🌙", ["dark-mode-system"] = "🌙", ["accent-color-title-bar"] = "🎨",
        ["accent-color-start-taskbar"] = "🎨", ["drag-full-window"] = "🖱", ["font-smoothing"] = "🔤",
        ["fast-menu"] = "⚡", ["snap-windows"] = "🪟", ["wallpaper-compression"] = "🖼",
        // 任务栏
        ["taskbar-combine"] = "📌", ["disable-widgets"] = "📋", ["disable-search-highlights"] = "🔍",
        ["hide-taskbar-search"] = "🔍", ["hide-taskview"] = "👥", ["taskbar-seconds"] = "🕐",
        ["taskbar-alignment-left"] = "⬅", ["taskbar-badge"] = "🔔",
        ["taskbar-tray-icons-all"] = "📌", ["disable-taskbar-flashing"] = "💡",
        ["disable-taskbar-thumbnails"] = "🪟",
        // 开始菜单
        ["disable-suggestions-in-start"] = "🚫", ["disable-cortana"] = "🎤",
        ["start-hide-recent-items"] = "📄", ["start-hide-recently-added"] = "📦", ["start-show-most-used"] = "⭐",
        // 系统界面
        ["enable-numlock"] = "🔢", ["disable-sticky-keys"] = "⌨",
        ["disable-filter-keys"] = "⌨", ["disable-toggle-keys"] = "🔊", ["scroll-direction"] = "🖱",
        // 隐私与搜索
        ["ad-id"] = "📢", ["telemetry"] = "📡", ["activity-history"] = "🕐",
        ["disable-copilot"] = "🤖", ["disable-tips"] = "💡",
        ["disable-tailored-experiences"] = "📊", ["disable-find-my-device"] = "📍",
        ["disable-cloud-search"] = "☁", ["disable-web-search"] = "🌐", ["disable-activity-feed"] = "📋",
        // 性能与电源
        ["game-mode"] = "🎮", ["vbs"] = "🧠", ["power-plan-high"] = "⚡",
        ["power-plan-balanced"] = "🔋", ["power-plan-saver"] = "💤",
        ["disable-autoplay"] = "💿", ["disable-sysmain"] = "💾", ["disable-wsearch"] = "🔍",
        ["disable-hibernate"] = "💤", ["disable-background-apps"] = "📱",
        ["disable-visual-effects"] = "✨", ["disable-prefetch"] = "⚡",
        ["enable-long-paths"] = "📏", ["disable-notification-tips"] = "🔕",
        ["disable-xbox-services"] = "🎮",
        // 安全与更新
        ["noauto-update"] = "⏸", ["update-fully-disable"] = "🚫",
        ["disable-defender"] = "🛡", ["defender-cloud-protection"] = "☁",
        ["defender-tamper-protection"] = "🔐", ["defender-exclusion-path"] = "📁",
        ["disable-smartscreen"] = "🖥", ["disable-telemetry"] = "📡",
        ["disable-pua-protection"] = "⚠",
        // 桌面
        ["show-network-desktop"] = "🌐", ["show-control-panel-desktop"] = "⚙",
        ["show-user-folder-desktop"] = "👤", ["hide-desktop-icons"] = "🖥",
        // 资源管理器补充
        ["auto-complete-path"] = "⌨", ["show-preview-pane"] = "👁",
        // 隐私与搜索补充
        ["disable-error-reporting"] = "🐛", ["disable-feedback-frequency"] = "📋",
        ["disable-onedrive"] = "☁", ["disable-app-diagnostics"] = "🔬",
        ["disable-location"] = "📍",
        // 任务栏补充
        ["disable-news-interests"] = "📰", ["taskbar-search-icon"] = "🔍",
    };

    public SystemSettingsProViewModel()
    {
        LoadSettings();
        Presets = _service.GetPresetDefinitions();
    }

    private void LoadSettings()
    {
        _allSettings = _service.GetAllSettings();
        foreach (var setting in _allSettings)
        {
            if (IconMap.TryGetValue(setting.Id, out var icon))
                setting.IconCode = icon;
            setting.PropertyChanged += OnSettingPropertyChanged;
        }
        _suppressChanges = true;
        var activePlanId = _service.GetCurrentPowerPlan();
        if (!string.IsNullOrEmpty(activePlanId))
        {
            var plan = _allSettings.Find(s => s.Id == activePlanId);
            if (plan != null) plan.IsEnabled = true;
        }
        _suppressChanges = false;
        FilterGroups();
    }

    private void OnSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressChanges) return;
        if (sender is not SystemSettingItem setting) return;
        if (!SettingApplyGuard.TryEnter()) return;
        try
        {
            _suppressChanges = true;
            if (setting.Id.StartsWith("power-plan-")) ApplyPowerPlan(setting);
            else ApplyChange(setting);
            _suppressChanges = false;
        }
        finally { SettingApplyGuard.Exit(); }
    }

    private void FilterGroups()
    {
        Groups.Clear();
        var categoryOrder = Categories.Skip(1).ToList();
        foreach (var cat in categoryOrder)
        {
            if (_selectedCategory != "全部" && cat != _selectedCategory) continue;
            var group = new SettingGroup { CategoryName = cat };
            foreach (var setting in _allSettings)
            {
                if (setting.Category != cat) continue;
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    if (!setting.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                        !setting.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                        !setting.RiskLabel.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                group.Items.Add(setting);
            }
            if (group.Items.Count > 0)
                Groups.Add(group);
        }
    }

    private void ApplyChange(SystemSettingItem setting)
    {
        if (setting.Id == "defender-tamper-protection")
        {
            setting.IsEnabled = !setting.IsEnabled;
            MessageBox.Show("篡改保护无法通过注册表修改。\n请打开「Windows 安全中心 → 病毒和威胁防护 → 管理设置」手动关闭。",
                "仅可手动操作", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var result = _service.ApplySetting(setting.Id, setting.IsEnabled);
        if (!result)
        {
            setting.IsEnabled = !setting.IsEnabled;
            MessageBox.Show($"「{setting.Name}」设置失败，请以管理员身份运行。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyPowerPlan(SystemSettingItem target)
    {
        if (!target.IsEnabled) { target.IsEnabled = true; return; }
        foreach (var s in _allSettings)
            if (s.Id.StartsWith("power-plan-") && s.Id != target.Id)
                s.IsEnabled = false;
        var result = _service.ApplySetting(target.Id, true);
        if (!result)
        {
            target.IsEnabled = false;
            MessageBox.Show($"「{target.Name}」设置失败，请以管理员身份运行。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async void ApplyPreset(PresetDefinition preset)
    {
        var result = MessageBox.Show(
            $"即将应用「{preset.Name}」预设方案\n\n{preset.Description}\n\n将调整 {preset.TargetStates.Count} 项设置，确定继续？",
            "应用预设", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        IsBusy = true;
        _suppressChanges = true;

        var total = preset.TargetStates.Count;
        var applied = 0;
        foreach (var (id, enabled) in preset.TargetStates)
        {
            var setting = _allSettings.Find(s => s.Id == id);
            if (setting == null) continue;

            setting.IsEnabled = enabled;
            var ok = _service.ApplySetting(id, enabled);
            if (ok) applied++;

            await Task.Delay(50);
        }

        _suppressChanges = false;
        FilterGroups();
        IsBusy = false;

        MessageBox.Show($"「{preset.Name}」应用完成：成功 {applied}/{total} 项。",
            "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void RestoreCategory(string category)
    {
        var ids = _allSettings
            .Where(s => s.Category == category)
            .Select(s => s.Id);

        var result = MessageBox.Show(
            $"即将恢复「{category}」类别中的所有设置为默认值。\n\n确定继续？",
            "恢复默认", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        _suppressChanges = true;
        _service.RestoreCategory(ids);
        // Refresh state from registry
        _allSettings = _service.GetAllSettings();
        foreach (var setting in _allSettings)
        {
            if (IconMap.TryGetValue(setting.Id, out var icon))
                setting.IconCode = icon;
        }
        _suppressChanges = false;
        FilterGroups();
    }

    public void RestoreAll()
    {
        var result = MessageBox.Show(
            "即将恢复「所有」系统设置为默认值。\n\n此操作不可撤销，确定继续？",
            "恢复全部默认", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        _suppressChanges = true;
        foreach (var setting in _allSettings)
            _service.RevertSetting(setting.Id);

        _allSettings = _service.GetAllSettings();
        foreach (var setting in _allSettings)
        {
            if (IconMap.TryGetValue(setting.Id, out var icon))
                setting.IconCode = icon;
        }
        _suppressChanges = false;
        FilterGroups();
    }
}
