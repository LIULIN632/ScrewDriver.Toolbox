using System.Collections.ObjectModel;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class OptimizeDetailViewModel
{
    public ObservableCollection<SystemSettingItem> SettingItems { get; } = new();

    public void LoadSettingsByCategory(string categoryName)
    {
        SettingItems.Clear();

        switch (categoryName)
        {
            case "隐私和安全": LoadPrivacySettings(); break;
            case "电源": LoadPowerSettings(); break;
            case "游戏和性能": LoadGameSettings(); break;
            case "更新": LoadUpdateSettings(); break;
            case "通知": LoadNotifySettings(); break;
            case "声音": LoadSoundSettings(); break;
            case "系统设置": LoadSystemSettings(); break;
        }
    }

    private void LoadPrivacySettings()
    {
        SettingItems.Add(new() { Name = "关闭广告标识符", Description = "禁止应用使用广告 ID 跟踪你的行为", IconCode = "🛡️", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭活动历史记录", Description = "禁止 Windows 记录你的设备使用活动", IconCode = "📋", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "禁用位置服务", Description = "全局关闭系统定位服务，所有应用无法获取位置", IconCode = "📍", RiskLevel = RiskLevel.Dangerous });
        SettingItems.Add(new() { Name = "关闭遥测数据", Description = "禁止向微软发送诊断和使用数据", IconCode = "📊", RiskLevel = RiskLevel.Optional });
    }

    private void LoadPowerSettings()
    {
        SettingItems.Add(new() { Name = "开启高性能电源模式", Description = "提升性能，增加功耗，适合插电使用", IconCode = "⚡", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭休眠功能", Description = "删除 hiberfil.sys 文件，释放磁盘空间", IconCode = "💤", RiskLevel = RiskLevel.Dangerous });
    }

    private void LoadGameSettings()
    {
        SettingItems.Add(new() { Name = "开启游戏模式", Description = "游戏时优先分配 CPU/GPU 资源", IconCode = "🎮", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭全屏优化", Description = "解决部分游戏全屏时帧率波动问题", IconCode = "🖥️", RiskLevel = RiskLevel.Optional });
    }

    private void LoadUpdateSettings()
    {
        SettingItems.Add(new() { Name = "暂停自动更新", Description = "临时暂停 Windows 更新 7 天", IconCode = "🔄", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭传递优化", Description = "禁止 P2P 分享更新包，节省带宽", IconCode = "📡", RiskLevel = RiskLevel.Optional });
    }

    private void LoadNotifySettings()
    {
        SettingItems.Add(new() { Name = "关闭弹窗通知", Description = "禁止所有应用弹出桌面通知", IconCode = "🔔", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭锁屏通知", Description = "锁屏界面不显示通知内容", IconCode = "🔒", RiskLevel = RiskLevel.Recommended });
    }

    private void LoadSoundSettings()
    {
        SettingItems.Add(new() { Name = "关闭系统提示音", Description = "关闭开机、操作等所有系统音效", IconCode = "🔇", RiskLevel = RiskLevel.Recommended });
    }

    private void LoadSystemSettings()
    {
        SettingItems.Add(new() { Name = "系统还原", Description = "打开系统还原设置，创建或恢复还原点", IconCode = "🔄", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "环境变量", Description = "编辑系统环境变量，配置 PATH 等", IconCode = "📋", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "高级系统设置", Description = "性能、用户配置文件、启动和故障恢复", IconCode = "⚙️", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "远程桌面", Description = "配置远程桌面连接和远程协助", IconCode = "🖥️", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "系统保护", Description = "配置系统保护、磁盘空间管理和还原点", IconCode = "🛡️", RiskLevel = RiskLevel.Optional });
    }
}
