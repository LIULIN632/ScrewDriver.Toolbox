using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SystemSettingViewModel
{
    public ObservableCollection<OptimizeCategoryItem> Items { get; } = new();

    public SystemSettingViewModel()
    {
        AddItem("系统还原", "创建或恢复系统还原点", "🔄", "systempropertiesprotection");
        AddItem("环境变量", "编辑系统环境变量", "📋", "sysdm.cpl");
        AddItem("高级系统设置", "性能、用户配置文件、启动和故障恢复", "⚙️", "systempropertiesadvanced");
        AddItem("远程桌面", "配置远程桌面连接", "🖥️", "sysdm.cpl");
        AddItem("任务栏", "自定义任务栏行为和图标", "📌", "ms-settings:taskbar");
        AddItem("桌面", "桌面图标、右键菜单和显示设置", "🖥️", "ms-settings:personalization");
        AddItem("开始菜单", "开始菜单布局、推荐内容设置", "🏠", "ms-settings:personalization-start");
        AddItem("主题", "主题、颜色、壁纸和锁屏背景", "🎨", "ms-settings:themes");
        AddItem("系统信息", "查看系统硬件和软件环境详情", "ℹ️", "msinfo32");
        AddItem("设备管理器", "查看和管理硬件设备", "🔧", "devmgmt.msc");
    }

    private void AddItem(string title, string desc, string icon, string cmd)
    {
        Items.Add(new OptimizeCategoryItem
        {
            Title = title,
            Description = desc,
            IconCode = icon,
            NavigateCommand = new RelayCommand(_ =>
            {
                try { Process.Start(new ProcessStartInfo(cmd) { UseShellExecute = true }); }
                catch { }
            })
        });
    }
}
