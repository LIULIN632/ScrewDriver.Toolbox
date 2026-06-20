using System.Collections.ObjectModel;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.UI.Common;
using ScrewDriver.Toolbox.UI.Views.Pages;
using WpfApp = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class PresetsViewModel : BaseViewModel
{
    public ObservableCollection<PresetItem> PresetList { get; } = new();

    public PresetsViewModel()
    {
        InitPresets();
    }

    private void InitPresets()
    {
        PresetList.Add(new PresetItem
        {
            Name = "新机推荐", Tag = "推荐 · 安全",
            Description = "一键启用所有推荐的安全优化项，关闭广告跟踪、遥测数据、活动历史，适合新电脑或重装系统后初始化使用。",
            IconCode = "🛡️",
            Scene = "新装系统/重装系统后初始化，追求隐私安全无广告，不改动系统核心功能",
            Effect = "关闭广告跟踪、遥测收集、冗余弹窗，降低后台隐私泄露风险，无副作用",
            Notice = "仅修改系统界面和隐私配置，不影响系统更新、安全防护、硬件驱动，新手可放心使用",
            SettingKeys = new() { "ad-id", "telemetry", "activity-history", "disable-tips",
                "disable-copilot", "disable-web-search", "disable-feedback-frequency" },
        });
        PresetList[0].ApplyCommand = new RelayCommand(_ => ApplyPreset(PresetList[0]));
        PresetList[0].ViewDetailCommand = new RelayCommand(_ => ShowDetail(PresetList[0]));

        PresetList.Add(new PresetItem
        {
            Name = "极简模式", Tag = "界面精简",
            Description = "关闭小组件、搜索框、动画效果、任务视图，精简系统界面，降低资源占用，追求纯净桌面体验。",
            IconCode = "🎯",
            Scene = "追求纯净桌面体验，不需要小组件、搜索框等冗余功能",
            Effect = "精简任务栏、开始菜单、资源管理器，减少UI动画资源占用，桌面更清爽",
            Notice = "所有界面修改均可一键恢复，不会破坏系统文件，部分设置需重启资源管理器生效",
            SettingKeys = new() { "disable-widgets", "hide-taskview", "classic-context",
                "show-extensions", "show-hidden-files", "disable-animation" },
        });
        PresetList[1].ApplyCommand = new RelayCommand(_ => ApplyPreset(PresetList[1]));
        PresetList[1].ViewDetailCommand = new RelayCommand(_ => ShowDetail(PresetList[1]));

        PresetList.Add(new PresetItem
        {
            Name = "老电脑优化", Tag = "性能优先",
            Description = "关闭动画效果、启用高性能电源计划、禁用 VBS，最大化释放硬件性能，适合低配置老旧设备。",
            IconCode = "⚡",
            Scene = "低配置老旧电脑、机械硬盘设备，优先保证运行流畅度",
            Effect = "释放 10%-20% 内存和磁盘占用，提升开机和操作流畅度",
            Notice = "关闭 VBS 会降低系统安全性，但可显著提升游戏和日常使用性能。所有设置均可恢复",
            SettingKeys = new() { "power-plan-high", "disable-animation", "vbs", "disable-tips" },
        });
        PresetList[2].ApplyCommand = new RelayCommand(_ => ApplyPreset(PresetList[2]));
        PresetList[2].ViewDetailCommand = new RelayCommand(_ => ShowDetail(PresetList[2]));
    }

    private void ApplyPreset(PresetItem preset)
    {
        var result = MessageBox.Show($"即将应用预设方案「{preset.Name}」\n\n包含 {preset.SettingKeys.Count} 项设置\n\n确定继续？",
            "确认应用", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (result != MessageBoxResult.OK) return;

        int success = 0;
        int fail = 0;
        foreach (var key in preset.SettingKeys)
        {
            if (RegistryHelper.ApplySettingById(key, true))
                success++;
            else
                fail++;
        }

        MessageBox.Show($"预设「{preset.Name}」应用完成\n成功 {success} 项 / 失败 {fail} 项\n部分设置需重启系统生效",
            "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowDetail(PresetItem preset)
    {
        var detail = new PresetDetailWindow(preset);
        if (WpfApp.Current.MainWindow is Window main)
            detail.Owner = main;
        detail.ShowDialog();
    }
}
