using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.SystemTools.Services;
using ScrewDriver.Toolbox.UI.Common;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class OptimizeDetailViewModel
{
    private readonly SystemOptimizerService _service = new();
    private readonly string _categoryName;
    private SystemSettingItem? _lastToggled;

    public ObservableCollection<SystemSettingItem> SettingItems { get; } = new();

    public ObservableCollection<RegistryComboItem> ComboItems { get; } = new();

    public OptimizeDetailViewModel()
    {
        _categoryName = "";
    }

    public OptimizeDetailViewModel(string categoryName) : this()
    {
        _categoryName = categoryName;
        LoadSettings();
    }

    public void LoadSettings()
    {
        SettingItems.Clear();

        // 从服务加载所有设置
        var allSettings = _service.GetAllSettings();
        var categorySettings = allSettings.Where(s => s.Category == _categoryName).ToList();

        if (categorySettings.Count > 0)
        {
            // 有对应服务定义，直接使用并监听变更
            foreach (var setting in categorySettings)
            {
                setting.PropertyChanged += (_, _) =>
                {
                    if (setting == _lastToggled)
                        ApplyChange(setting);
                };
                SettingItems.Add(setting);
            }
        }
        else
        {
            // 没有服务定义 → 创建未关联的设置项，提示无法操作
            LoadFallbackSettings();
        }

        // 额外注册表设置（Dism++ 规则）
        AddRegistrySettings();
        AddComboSettings();
    }

    private void ApplyChange(SystemSettingItem setting)
    {
        var result = _service.ApplySetting(setting.Id, setting.IsEnabled);
        if (!result)
        {
            setting.IsEnabled = !setting.IsEnabled;
            MessageBox.Show($"「{setting.Name}」设置失败，请以管理员身份运行。", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadFallbackSettings()
    {
        var map = new (string id, string name, string desc, string icon)[]
        {
            ("ad-id", "关闭广告 ID", "禁止应用使用广告 ID 跟踪你的行为", "🛡️"),
            ("telemetry", "关闭诊断数据", "将诊断数据收集级别设为最低", "📊"),
            ("activity-history", "关闭活动历史记录", "禁止 Windows 记录使用活动", "📋"),
            ("show-extensions", "显示文件扩展名", "资源管理器中显示文件扩展名", "📄"),
            ("show-hidden-files", "显示隐藏文件", "资源管理器中显示隐藏文件", "📁"),
            ("classic-context", "经典右键菜单", "恢复 Win10 风格完整右键菜单", "📋"),
            ("disable-widgets", "关闭小组件", "禁用任务栏小组件按钮", "📰"),
            ("disable-search-highlights", "关闭搜索亮点", "禁用搜索框每日亮点", "🔍"),
            ("game-mode", "游戏模式", "启用 Windows 游戏模式", "🎮"),
            ("vbs", "关闭 VBS 内存完整性", "禁用虚拟化安全提升游戏性能", "🧠"),
            ("disable-copilot", "关闭 Copilot", "移除任务栏 Copilot 按钮", "🤖"),
            ("disable-tips", "关闭 Windows 提示", "禁用设置建议和锁屏提示", "💡"),
            ("power-plan-high", "高性能电源计划", "切换至高性能电源模式", "⚡"),
            ("power-plan-balanced", "平衡电源计划", "兼顾性能与功耗", "🔋"),
            ("power-plan-saver", "节能电源计划", "最大化电池续航", "💤"),
            ("noauto-update", "暂停自动更新", "临时暂停 Windows 更新", "🔄"),
            ("disable-defender", "关闭 Defender", "禁用实时保护（高风险）", "🛡️"),
        };

        var allSettings = _service.GetAllSettings();

        foreach (var (id, name, desc, icon) in map)
        {
            var matched = allSettings.FirstOrDefault(s => s.Id == id);
            if (matched != null)
            {
                matched.IconCode = icon;
                matched.PropertyChanged += (_, _) =>
                {
                    if (matched == _lastToggled)
                        ApplyChange(matched);
                };
                SettingItems.Add(matched);
            }
        }

        // 补充来自 Dism++ 规则的注册表设置
        AddRegistrySettings();
    }

    /// <summary>添加基于注册表的额外优化项（从 Dism++ Data.xml 提取）</summary>
    private void AddRegistrySettings()
    {
        // 资源管理器设置
        AddRegSetting("隐藏任务视图按钮", "隐藏任务栏的任务视图按钮", "📌",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton");
        AddRegSetting("任务栏使用小图标", "任务栏使用小图标节省空间", "📌",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarSmallIcons");
        AddRegSetting("显示桌面图标", "在桌面上显示此电脑/回收站等图标", "🖥️",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", 0);
        AddRegSetting("关闭动画效果", "关闭窗口淡入淡出动画，提升响应速度", "🎬",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DisableAnimations");
        
        // 系统策略
        AddRegSetting("关闭自动播放", "禁止自动播放U盘和光盘等媒体", "💿",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun");
        AddRegSetting("关闭系统通知", "禁止操作系统托盘的通知图标", "🔔",
            @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter");
        
        // Windows 设置
        AddRegSetting("关闭透明效果", "关闭任务栏和开始菜单的亚克力透明效果", "🎨",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency");
        AddRegSetting("关闭搜索框建议", "在搜索框中不显示建议内容", "🔍",
            @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions");
        
        // 资源管理器增强
        AddRegSetting("显示系统文件", "在资源管理器中显示受保护的操作系统文件", "📁",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden");
        AddRegSetting("关闭缩略图缓存", "禁止资源管理器缓存缩略图", "🖼️",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DisableThumbnailCache");
        AddRegSetting("展开到当前文件夹", "导航窗格自动展开到当前文件夹", "📂",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "NavPaneExpandToCurrentFolder");
    }

    private void AddRegSetting(string name, string desc, string icon, string subKey, string valueName, int trueVal = 1)
    {
        var item = new SystemSettingItem
        {
            Name = name,
            Description = desc,
            IconCode = icon,
            RiskLevel = RiskLevel.Optional,
            IsEnabled = RegistryOptimizer.ReadBool(subKey, valueName, trueVal)
        };

        // 添加变更事件
        item.PropertyChanged += (_, _) =>
        {
            if (item == _lastToggled)
            {
                var result = RegistryOptimizer.WriteBool(subKey, valueName, item.IsEnabled, trueVal);
                if (!result)
                {
                    item.IsEnabled = !item.IsEnabled;
                    MessageBox.Show($"「{item.Name}」设置失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        };

        SettingItems.Add(item);
    }

    public void ToggleSetting(SystemSettingItem setting)
    {
        _lastToggled = setting;
        var result = _service.ApplySetting(setting.Id, setting.IsEnabled);
        if (!result)
        {
            setting.IsEnabled = !setting.IsEnabled;
            MessageBox.Show($"「{setting.Name}」设置失败，请以管理员身份运行。", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddComboSettings()
    {
        // 任务栏搜索框样式
        var searchBox = new RegistryComboItem
        {
            Name = "任务栏搜索框",
            Description = "控制任务栏搜索框的显示方式",
            IconCode = "🔍",
            SubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search",
            ValueName = "SearchboxTaskbarMode"
        };
        searchBox.Options.Add(new() { DisplayName = "隐藏", Value = 0 });
        searchBox.Options.Add(new() { DisplayName = "仅显示图标", Value = 1 });
        searchBox.Options.Add(new() { DisplayName = "显示搜索框", Value = 2 });
        ComboItems.Add(searchBox);

        // 任务栏合并
        var combine = new RegistryComboItem
        {
            Name = "任务栏按钮合并",
            Description = "控制任务栏上按钮的合并行为",
            IconCode = "📌",
            SubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            ValueName = "TaskbarGlomLevel"
        };
        combine.Options.Add(new() { DisplayName = "始终合并隐藏标签", Value = 0 });
        combine.Options.Add(new() { DisplayName = "任务栏已满时合并", Value = 1 });
        combine.Options.Add(new() { DisplayName = "从不合并", Value = 2 });
        ComboItems.Add(combine);
    }
}
