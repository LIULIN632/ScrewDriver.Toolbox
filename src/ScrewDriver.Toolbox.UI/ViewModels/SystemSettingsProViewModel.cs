using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.SystemTools.Services;
using ScrewDriver.Toolbox.UI.Common;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SystemSettingsProViewModel : BaseViewModel
{
    private readonly SystemOptimizerService _service = new();
    private bool _suppressChanges;
    private string _selectedCategory = "全部";
    private string _searchText = string.Empty;
    private List<SystemSettingItem> _allSettings = new();
    private int _pendingExplorerRestartCount;
    private readonly HashSet<string> _pendingRestartIds = new();

    public ObservableCollection<SettingGroup> Groups { get; } = new();

    public int PendingExplorerRestartCount
    {
        get => _pendingExplorerRestartCount;
        set => SetProperty(ref _pendingExplorerRestartCount, value);
    }

    public ICommand RestartExplorerCommand { get; }

    private static readonly HashSet<string> ExplorerRestartIds = new()
    {
        "show-extensions", "show-hidden-files", "show-system-files", "expand-to-current-folder",
        "classic-context", "disable-quick-access-history", "show-full-path-in-title",
        "show-status-bar", "open-to-pc", "hide-common-folders", "nav-pane-all-folders",
        "show-libraries", "compact-mode", "check-box-select", "show-drive-letters-before",
        "show-encrypted-compressed-color", "launch-folder-windows-separate", "hide-sync-provider",
        "disable-thumbnails", "disable-thumbnail-cache", "auto-complete-path", "show-preview-pane",
        "show-this-pc", "show-recycle-bin", "show-network-desktop", "show-control-panel-desktop",
        "show-user-folder-desktop", "hide-desktop-icons",
        "disable-transparency", "dark-mode-system", "accent-color-start-taskbar",
        "taskbar-combine", "disable-widgets", "disable-search-highlights", "hide-taskbar-search",
        "hide-taskview", "taskbar-seconds", "taskbar-alignment-left", "taskbar-badge",
        "taskbar-tray-icons-all", "disable-taskbar-flashing", "disable-taskbar-thumbnails",
        "disable-news-interests", "taskbar-search-icon",
        "disable-cortana", "disable-suggestions-in-start", "start-hide-recent-items",
        "start-hide-recently-added", "start-show-most-used",
    };

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

    internal static readonly Dictionary<string, string> IconMap = new()
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
        RestartExplorerCommand = new RelayCommand(_ => ExecuteRestartExplorer());
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

    private async void OnSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressChanges) return;
        if (sender is not SystemSettingItem setting) return;
        if (!SettingApplyGuard.TryEnter()) return;
        IsBusy = true;
        try
        {
            _suppressChanges = true;
            if (setting.Id.StartsWith("power-plan-"))
                await ApplyPowerPlanAsync(setting);
            else
                await ApplyChangeAsync(setting);
        }
        finally
        {
            _suppressChanges = false;
            IsBusy = false;
            SettingApplyGuard.Exit();
        }
    }

    private void FilterGroups()
    {
        Groups.Clear();
        var query = SearchText?.Trim();
        var categoryOrder = Categories.Skip(1).ToList();
        foreach (var cat in categoryOrder)
        {
            if (_selectedCategory != "全部" && cat != _selectedCategory) continue;
            var group = new SettingGroup { CategoryName = cat };
            foreach (var setting in _allSettings)
            {
                if (setting.Category != cat) continue;
                if (!string.IsNullOrWhiteSpace(query) && !MatchesSearch(setting, query))
                    continue;
                group.Items.Add(setting);
            }
            if (group.Items.Count > 0)
                Groups.Add(group);
        }
    }

    private static bool MatchesSearch(SystemSettingItem item, string query)
    {
        if (item.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            item.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            item.RiskLabel.Contains(query, StringComparison.OrdinalIgnoreCase))
            return true;

        if (query.Length >= 2 && query.All(c => c is >= 'a' and <= 'z'))
        {
            var py = GetPinyinInitials(item.Name);
            if (py.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string GetPinyinInitials(string text)
    {
        var result = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            if (PinyinMap.TryGetValue(c, out var py))
                result.Append(py);
        }
        return result.ToString().ToLowerInvariant();
    }

    private static readonly Dictionary<char, string> PinyinMap = new()
    {
        // A
        ['安']="a", ['暗']="a",
        // B
        ['保']="b", ['壁']="b", ['编']="b", ['标']="b", ['不']="b", ['保']="b", ['播']="b",
        // C
        ['菜']="c", ['仓']="c", ['窗']="c", ['此']="c", ['存']="c", ['操']="c", ['侧']="c",
        ['查']="c", ['程']="c", ['重']="c", ['处']="c", ['触']="c", ['磁']="c",
        // D
        ['打']="d", ['代']="d", ['单']="d", ['登']="d", ['地']="d", ['电']="d", ['定']="d",
        ['动']="d", ['独']="d", ['端']="d", ['多']="d", ['耽']="d", ['底']="d", ['弹']="d",
        ['低']="d", ['调']="d", ['大']="d", ['断']="d", ['对']="d",
        // E
        ['二']="e",
        // F
        ['方']="f", ['访']="f", ['放']="f", ['服']="f", ['复']="f", ['反']="f", ['分']="f",
        ['粉']="f",
        // G
        ['高']="g", ['工']="g", ['共']="g", ['关']="g", ['管']="g", ['广']="g", ['个']="g",
        ['功']="g", ['故']="g", ['更']="g", ['格']="g",
        // H
        ['合']="h", ['后']="h", ['护']="h", ['画']="h", ['缓']="h", ['恢']="h", ['忽']="h",
        ['会']="h", ['活']="h", ['获']="h",
        // J
        ['计']="j", ['记']="j", ['加']="j", ['加']="j", ['检']="j", ['建']="j", ['键']="j",
        ['将']="j", ['接']="j", ['节']="j", ['界']="j", ['禁']="j", ['经']="j", ['警']="j",
        ['拒']="j", ['聚']="j", ['角']="j", ['较']="j", ['仅']="j", ['基']="j", ['加']="j",
        ['即']="j", ['减']="j", ['兼']="j",
        // K
        ['开']="k", ['看']="k", ['可']="k", ['空']="k", ['控']="k", ['快']="k", ['扩']="k",
        // L
        ['离']="l", ['历']="l", ['联']="l", ['链']="l", ['列']="l", ['浏']="l", ['路']="l",
        ['录']="l",
        // M
        ['免']="m", ['面']="m", ['名']="m", ['模']="m", ['默']="m", ['媒']="m", ['密']="m",
        // N
        ['内']="n",
        // P
        ['排']="p", ['盘']="p", ['配']="p", ['频']="p", ['屏']="p", ['平']="p",
        // Q
        ['启']="q", ['前']="q", ['强']="q", ['清']="q", ['取']="q", ['全']="q", ['权']="q",
        ['确']="q", ['轻']="q", ['切']="q", ['区']="q",
        // R
        ['任']="r", ['日']="r",
        // S
        ['色']="s", ['删']="s", ['设']="s", ['深']="s", ['生']="s", ['声']="s", ['时']="s",
        ['使']="s", ['示']="s", ['事']="s", ['视']="s", ['收']="s", ['输']="s", ['数']="s",
        ['双']="s", ['睡']="s", ['速']="s", ['缩']="s", ['锁']="s", ['搜']="s", ['所']="s",
        ['上']="s", ['升']="s", ['删']="s", ['闪']="s",
        // T
        ['台']="t", ['提']="t", ['体']="t", ['替']="t", ['条']="t", ['停']="t", ['通']="t",
        ['同']="t", ['图']="t", ['推']="t", ['拖']="t", ['透']="t", ['调']="t", ['弹']="t",
        // W
        ['外']="w", ['网']="w", ['位']="w", ['文']="w", ['无']="w", ['物']="w",
        // X
        ['系']="x", ['显']="x", ['限']="x", ['香']="x", ['项']="x", ['小']="x", ['效']="x",
        ['新']="x", ['信']="x", ['性']="x", ['修']="x", ['选']="x", ['循']="x", ['息']="x",
        ['卸']="x",
        // Y
        ['压']="y", ['延']="y", ['颜']="y", ['遥']="y", ['页']="y", ['移']="y", ['已']="y",
        ['隐']="y", ['应']="y", ['用']="y", ['邮']="y", ['右']="y", ['预']="y", ['远']="y",
        ['音']="y", ['影']="y", ['硬']="y", ['游']="y", ['运']="y",
        // Z
        ['在']="z", ['暂']="z", ['增']="z", ['展']="z", ['站']="z", ['账']="z", ['照']="z",
        ['诊']="z", ['整']="z", ['正']="z", ['直']="z", ['指']="z", ['制']="z", ['中']="z",
        ['重']="z", ['周']="z", ['主']="z", ['注']="z", ['转']="z", ['装']="z", ['状']="z",
        ['资']="z", ['自']="z", ['字']="z", ['总']="z", ['组']="z", ['最']="z", ['作']="z",
        ['执']="z", ['置']="z", ['专']="z", ['桌']="z",
    };

    private async Task ApplyChangeAsync(SystemSettingItem setting)
    {
        if (setting.Id == "defender-tamper-protection")
        {
            setting.IsEnabled = !setting.IsEnabled;
            MessageBox.Show("篡改保护无法通过注册表修改。\n请打开「Windows 安全中心 → 病毒和威胁防护 → 管理设置」手动关闭。",
                "仅可手动操作", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var result = await Task.Run(() => _service.ApplySetting(setting.Id, setting.IsEnabled));
        if (!result)
        {
            setting.IsEnabled = !setting.IsEnabled;
            MessageBox.Show($"「{setting.Name}」设置失败，请以管理员身份运行。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else if (ExplorerRestartIds.Contains(setting.Id) && !_pendingRestartIds.Contains(setting.Id))
        {
            _pendingRestartIds.Add(setting.Id);
            PendingExplorerRestartCount = _pendingRestartIds.Count;
        }
    }

    private void ExecuteRestartExplorer()
    {
        var result = MessageBox.Show(
            $"有 {_pendingRestartIds.Count} 项设置的修改需要重启资源管理器才能生效。\n\n立即重启资源管理器？\n（所有打开的资源管理器窗口将关闭）",
            "重启资源管理器", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        ShellChangeNotifier.RestartExplorer();
        _pendingRestartIds.Clear();
        PendingExplorerRestartCount = 0;
    }

    private async Task ApplyPowerPlanAsync(SystemSettingItem target)
    {
        if (!target.IsEnabled) { target.IsEnabled = true; return; }
        foreach (var s in _allSettings)
            if (s.Id.StartsWith("power-plan-") && s.Id != target.Id)
                s.IsEnabled = false;
        var result = await Task.Run(() => _service.ApplySetting(target.Id, true));
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
            var ok = await Task.Run(() => _service.ApplySetting(id, enabled));
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
