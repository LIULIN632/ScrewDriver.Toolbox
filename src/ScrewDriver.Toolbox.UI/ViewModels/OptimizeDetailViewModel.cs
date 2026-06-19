using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.SystemTools.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class OptimizeDetailViewModel
{
    private readonly SystemOptimizerService _service = new();
    private readonly string _categoryName;
    private SystemSettingItem? _lastToggled;

    public ObservableCollection<SystemSettingItem> SettingItems { get; } = new();

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
}
