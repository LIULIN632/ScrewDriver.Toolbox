using System.Collections.ObjectModel;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);
    private BaseViewModel? _currentViewModel;
    private string _selectedNavTag = "StartPage";

    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string SelectedNavTag
    {
        get => _selectedNavTag;
        set { if (SetProperty(ref _selectedNavTag, value)) UpdateActiveState(); }
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();
    public ObservableCollection<NavigationItem> ToolCategories { get; } = new();
    public ObservableCollection<NavigationItem> SettingsCategories { get; } = new();

    public ICommand NavigateCommand { get; }
    public ICommand ToggleExpandCommand { get; }

    // Cached VMs for sub-category filtering
    private ToolRepositoryViewModel? _toolRepoVm;
    private SystemSettingsProViewModel? _settingsProVm;
    private SoftwareOptimizeViewModel? _softwareOptimizeVm;

    public MainViewModel()
    {
        NavigateCommand = new RelayCommand<string>(NavigateTo);
        ToggleExpandCommand = new RelayCommand<NavigationItem>(ToggleExpand);
        BuildNavigation();
        NavigateTo("StartPage");
    }

    private void BuildNavigation()
    {
        // 工具仓库分类
        foreach (var cat in ToolRegistry.Categories)
            ToolCategories.Add(new NavigationItem { Title = cat, Tag = $"Tool:{cat}" });

        // 系统设置 Pro 分类
        foreach (var cat in new[] { "一键预设", "个性化", "桌面", "资源管理器", "任务栏",
                                    "开始菜单", "系统界面", "隐私与搜索", "性能与电源", "安全与更新" })
            SettingsCategories.Add(new NavigationItem { Title = cat, Tag = $"Setting:{cat}" });

        NavigationItems.Add(new NavigationItem { Title = "启动", IconCode = "🏠", Tag = "StartPage" });
        NavigationItems.Add(new NavigationItem
        {
            Title = "工具仓库",
            IconCode = "📦",
            Tag = "ToolRepositoryPage",
            SubItems = new ObservableCollection<NavigationItem>(ToolCategories)
        });
        NavigationItems.Add(new NavigationItem
        {
            Title = "系统设置 Pro",
            IconCode = "⚙️",
            Tag = "SystemSettingsProPage",
            SubItems = new ObservableCollection<NavigationItem>(SettingsCategories)
        });
        NavigationItems.Add(new NavigationItem
        {
            Title = "软件优化",
            IconCode = "🛠",
            Tag = "SoftwareOptimizePage",
            SubItems = new ObservableCollection<NavigationItem>
            {
                new() { Title = "Edge", Tag = "Software:Edge" },
                new() { Title = "Office", Tag = "Software:Office" },
                new() { Title = "WPS", Tag = "Software:WPS" },
            }
        });
        NavigationItems.Add(new NavigationItem { Title = "系统修复", IconCode = "🔧", Tag = "RepairCenterPage" });
        NavigationItems.Add(new NavigationItem { Title = "硬件信息", IconCode = "💻", Tag = "HardwarePage" });
        NavigationItems.Add(new NavigationItem { Title = "设置", IconCode = "⚡", Tag = "SettingsPage" });

        UpdateActiveState();
    }

    public void NavigateTo(string? tag)
    {
        if (tag == null) return;
        SelectedNavTag = tag;

        if (tag.StartsWith("Tool:"))
        {
            var cat = tag[5..];
            _toolRepoVm ??= new ToolRepositoryViewModel();
            _toolRepoVm.SelectedCategory = cat;
            CurrentViewModel = _toolRepoVm;
            ExpandParent("ToolRepositoryPage");
            return;
        }

        if (tag.StartsWith("Setting:"))
        {
            CurrentViewModel = NavigateToSettingsPro(tag[8..]);
            ExpandParent("SystemSettingsProPage");
            return;
        }

        if (tag.StartsWith("Software:"))
        {
            _softwareOptimizeVm ??= new SoftwareOptimizeViewModel();
            _softwareOptimizeVm.SelectedSoftware = tag["Software:".Length..];
            CurrentViewModel = _softwareOptimizeVm;
            ExpandParent("SoftwareOptimizePage");
            return;
        }

        CurrentViewModel = tag switch
        {
            "StartPage" => new StartPageViewModel(),
            "ToolRepositoryPage" => (_toolRepoVm ??= new ToolRepositoryViewModel()),
            "SystemSettingsProPage" => NavigateToSettingsPro("全部"),
            "SoftwareOptimizePage" => (_softwareOptimizeVm ??= new SoftwareOptimizeViewModel()),
            "RepairCenterPage" => new RepairCenterViewModel(),
            "HardwarePage" => new HardwareViewModel(),
            "DataCenterPage" => new DataCenterViewModel(),
            "SettingsPage" => new SettingsViewModel(),
            _ => CurrentViewModel
        };

    }

    private BaseViewModel NavigateToSettingsPro(string category)
    {
        if (category == "一键预设")
            return new PresetsViewModel();
        _settingsProVm ??= new SystemSettingsProViewModel();
        _settingsProVm.SelectedCategory = category;
        return _settingsProVm;
    }

    private void ExpandParent(string parentTag)
    {
        foreach (var item in NavigationItems)
        {
            if (item.Tag == parentTag)
                item.IsExpanded = true;
        }
    }

    private void ToggleExpand(NavigationItem? item)
    {
        if (item?.SubItems.Count > 0)
        {
            item.IsExpanded = !item.IsExpanded;
            if (item.IsExpanded)
                NavigateTo(item.Tag);
        }
    }

    private void UpdateActiveState()
    {
        foreach (var item in NavigationItems)
        {
            item.IsActive = item.Tag == SelectedNavTag
                || item.SubItems.Any(s => s.Tag == SelectedNavTag);
            foreach (var sub in item.SubItems)
                sub.IsActive = sub.Tag == SelectedNavTag;
        }
    }
}
