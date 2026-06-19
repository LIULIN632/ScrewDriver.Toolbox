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
            case "隐私与体验": LoadPrivacySettings(); break;
            case "性能优化": LoadPerformanceSettings(); break;
            case "游戏优化": LoadGameSettings(); break;
            case "电源与电池": LoadPowerSettings(); break;
            case "存储与后台": LoadStorageSettings(); break;
            case "搜索与助手": LoadSearchSettings(); break;
            case "更新与传输": LoadUpdateSettings(); break;
            case "安全": LoadSecuritySettings(); break;
            case "通知与个性化": LoadNotifySettings(); break;
        }
    }

    private void LoadPrivacySettings()
    {
        SettingItems.Add(new() { Name = "关闭广告 ID", Description = "禁止应用使用广告 ID 跟踪你的行为", IconCode = "🛡️", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭诊断数据", Description = "禁止向微软发送诊断和使用数据", IconCode = "📊", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭活动历史记录", Description = "禁止 Windows 记录你的设备使用活动", IconCode = "📋", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭量身定制体验", Description = "不再根据使用习惯推荐内容和广告", IconCode = "🎯", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭云剪贴板", Description = "禁止跨设备同步剪贴板内容", IconCode = "📋", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭 Windows 反馈", Description = "禁止系统主动征求用户反馈意见", IconCode = "💬", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭 Windows 错误报告", Description = "禁止发送程序崩溃和错误报告", IconCode = "⚠️", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭 OneDrive 自动备份", Description = "禁止 OneDrive 自动备份桌面/文档/图片", IconCode = "☁️", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭 Copilot", Description = "隐藏任务栏 Copilot 按钮", IconCode = "🤖", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭定位服务", Description = "全局关闭系统定位服务", IconCode = "📍", RiskLevel = RiskLevel.Dangerous });
    }

    private void LoadPerformanceSettings()
    {
        SettingItems.Add(new() { Name = "开启 Ultimate Performance", Description = "启用终极性能电源计划，充分发挥硬件潜力", IconCode = "🚀", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "开启 HAGS", Description = "硬件加速 GPU 调度，降低游戏和渲染延迟", IconCode = "🎮", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭快速启动", Description = "关闭快速启动可解决关机/更新问题", IconCode = "⏩", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭动画效果", Description = "关闭窗口动画，提升低配机响应速度", IconCode = "🎬", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "开启核心隔离", Description = "启用 Core Isolation / VBS，提升系统安全性", IconCode = "🔒", RiskLevel = RiskLevel.Dangerous });
        SettingItems.Add(new() { Name = "关闭应用启动延迟", Description = "禁止系统延迟启动应用，适合高性能设备", IconCode = "⏰", RiskLevel = RiskLevel.Dangerous });
        SettingItems.Add(new() { Name = "关闭内存完整性", Description = "关闭内存完整性可提升游戏性能但降低安全性", IconCode = "🧠", RiskLevel = RiskLevel.Dangerous });
    }

    private void LoadGameSettings()
    {
        SettingItems.Add(new() { Name = "开启游戏模式", Description = "游戏时优先分配 CPU/GPU 资源", IconCode = "🎮", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "开启窗口化游戏优化", Description = "改善窗口/无边框模式游戏的帧率和延迟", IconCode = "🖥️", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭鼠标加速度", Description = "关闭鼠标加速，提升游戏瞄准精度", IconCode = "🖱️", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭全屏优化", Description = "解决部分游戏全屏时帧率波动问题", IconCode = "🖥️", RiskLevel = RiskLevel.Optional });
    }

    private void LoadPowerSettings()
    {
        SettingItems.Add(new() { Name = "开启高性能电源模式", Description = "提升性能，增加功耗，适合插电使用", IconCode = "⚡", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭休眠功能", Description = "删除 hiberfil.sys 文件，释放磁盘空间", IconCode = "💤", RiskLevel = RiskLevel.Dangerous });
        SettingItems.Add(new() { Name = "开启节能模式", Description = "降低性能以换取更长电池续航", IconCode = "🔋", RiskLevel = RiskLevel.Optional });
    }

    private void LoadStorageSettings()
    {
        SettingItems.Add(new() { Name = "开启存储感知", Description = "自动清理临时文件和回收站释放空间", IconCode = "🗑️", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭传递优化", Description = "禁止 P2P 分享更新包，节省带宽", IconCode = "📡", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭后台服务", Description = "优化后台服务，减少系统资源占用", IconCode = "⚙️", RiskLevel = RiskLevel.Dangerous });
    }

    private void LoadSearchSettings()
    {
        SettingItems.Add(new() { Name = "关闭搜索亮点", Description = "搜索框不再显示新闻和热点内容", IconCode = "🔍", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "关闭搜索历史记录", Description = "禁止记录此设备的搜索历史", IconCode = "📜", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭小娜", Description = "禁用 Windows 语音助手 Cortana", IconCode = "🗣️", RiskLevel = RiskLevel.Optional });
    }

    private void LoadUpdateSettings()
    {
        SettingItems.Add(new() { Name = "暂停自动更新", Description = "临时暂停 Windows 更新 7 天", IconCode = "🔄", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭驱动自动更新", Description = "禁止 Windows 自动更新驱动程序", IconCode = "🔧", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭商店自动更新", Description = "禁止 Microsoft Store 自动更新应用", IconCode = "🏪", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "清理更新缓存", Description = "清理 Windows Update 缓存文件释放空间", IconCode = "🧹", RiskLevel = RiskLevel.Optional });
    }

    private void LoadSecuritySettings()
    {
        SettingItems.Add(new() { Name = "关闭 UAC", Description = "关闭用户账户控制，不再弹窗确认", IconCode = "🛡️", RiskLevel = RiskLevel.Dangerous });
        SettingItems.Add(new() { Name = "关闭 SmartScreen", Description = "关闭 Windows Defender SmartScreen 保护", IconCode = "🖥️", RiskLevel = RiskLevel.Dangerous });
        SettingItems.Add(new() { Name = "关闭 BitLocker 自动加密", Description = "禁止设备自动启用 BitLocker 加密", IconCode = "🔐", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭 Defender", Description = "关闭 Windows Defender 防病毒保护", IconCode = "🛡️", RiskLevel = RiskLevel.Dangerous });
    }

    private void LoadNotifySettings()
    {
        SettingItems.Add(new() { Name = "关闭弹窗通知", Description = "禁止所有应用弹出桌面通知", IconCode = "🔔", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭锁屏通知", Description = "锁屏界面不显示通知内容，保护隐私", IconCode = "🔒", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "开启深色模式", Description = "切换系统为深色主题", IconCode = "🌙", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "显示文件扩展名", Description = "在资源管理器中显示文件扩展名", IconCode = "📄", RiskLevel = RiskLevel.Recommended });
        SettingItems.Add(new() { Name = "显示隐藏文件", Description = "在资源管理器中显示隐藏文件", IconCode = "📁", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭开机声音", Description = "禁止 Windows 启动时播放声音", IconCode = "🔇", RiskLevel = RiskLevel.Optional });
        SettingItems.Add(new() { Name = "关闭小组件", Description = "隐藏任务栏小组件按钮", IconCode = "📰", RiskLevel = RiskLevel.Optional });
    }
}
