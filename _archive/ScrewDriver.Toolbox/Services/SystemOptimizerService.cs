using ScrewDriver.Toolbox.Models;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.Services;

public class SystemOptimizerService : ISystemOptimizerService
{
    private readonly List<SettingItem> _settings = [];

    public SystemOptimizerService()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        // 基础设置
        _settings.Add(new SettingItem { Id = "update_enable", Title = "Windows 更新", Description = "启用/禁用 Windows 自动更新", Category = "基础设置", IsEnabled = true, RecommendedValue = true, IsDangerous = true, WarningMessage = "关闭更新可能导致安全风险" });
        _settings.Add(new SettingItem { Id = "fast_startup", Title = "快速启动", Description = "启用快速启动功能", Category = "基础设置", IsEnabled = true, RecommendedValue = true });
        _settings.Add(new SettingItem { Id = "night_light", Title = "夜间模式", Description = "启用夜间模式护眼", Category = "基础设置", IsEnabled = false, RecommendedValue = true });

        // 隐私设置
        _settings.Add(new SettingItem { Id = "telemetry", Title = "关闭遥测", Description = "禁止向微软发送诊断数据", Category = "隐私设置", IsEnabled = true, RecommendedValue = false, IsDangerous = false });
        _settings.Add(new SettingItem { Id = "ad_id", Title = "关闭广告 ID", Description = "禁止应用使用广告 ID", Category = "隐私设置", IsEnabled = true, RecommendedValue = false });
        _settings.Add(new SettingItem { Id = "copilot", Title = "关闭 Copilot", Description = "禁用 Windows Copilot 功能", Category = "隐私设置", IsEnabled = false, RecommendedValue = false });

        // 资源管理器
        _settings.Add(new SettingItem { Id = "show_ext", Title = "显示文件扩展名", Description = "在资源管理器中显示文件扩展名", Category = "资源管理器", IsEnabled = false, RecommendedValue = true });
        _settings.Add(new SettingItem { Id = "show_hidden", Title = "显示隐藏文件", Description = "在资源管理器中显示隐藏文件", Category = "资源管理器", IsEnabled = false, RecommendedValue = true });
        _settings.Add(new SettingItem { Id = "classic_menu", Title = "经典右键菜单", Description = "恢复 Win10 经典右键菜单样式", Category = "资源管理器", IsEnabled = false, RecommendedValue = true });

        // Defender 管理
        _settings.Add(new SettingItem { Id = "defender_realtime", Title = "Defender 实时保护", Description = "启用/禁用 Defender 实时保护", Category = "Defender 管理", IsEnabled = true, RecommendedValue = true, IsDangerous = true });
        _settings.Add(new SettingItem { Id = "defender_cloud", Title = "Defender 云保护", Description = "启用/禁用 Defender 云保护", Category = "Defender 管理", IsEnabled = true, RecommendedValue = true });

        // 性能优化
        _settings.Add(new SettingItem { Id = "vbs_disable", Title = "关闭 VBS", Description = "关闭内存完整性(VBS)提升游戏性能", Category = "性能优化", IsEnabled = false, RecommendedValue = false, IsDangerous = true, WarningMessage = "关闭 VBS 会降低系统安全级别" });
        _settings.Add(new SettingItem { Id = "game_mode", Title = "游戏模式", Description = "启用 Windows 游戏模式", Category = "性能优化", IsEnabled = true, RecommendedValue = true });
        _settings.Add(new SettingItem { Id = "bg_apps", Title = "后台应用限制", Description = "限制后台应用运行", Category = "性能优化", IsEnabled = false, RecommendedValue = true });
    }

    public ObservableCollection<string> GetCategories() =>
        ["基础设置", "隐私设置", "资源管理器", "Defender 管理", "性能优化"];

    public ObservableCollection<SettingItem> GetSettingsByCategory(string category) =>
        new(_settings.Where(s => s.Category == category));

    public bool ApplySetting(SettingItem item, bool enable)
    {
        // TODO: 实际执行系统设置变更
        item.IsEnabled = enable;
        item.HasPendingChange = false;
        return true;
    }

    public bool RestoreSetting(string settingId)
    {
        var item = _settings.FirstOrDefault(s => s.Id == settingId);
        if (item == null) return false;
        item.IsEnabled = item.RecommendedValue;
        return true;
    }

    public bool CreateRestorePoint()
    {
        // TODO: 创建系统还原点
        return true;
    }
}
