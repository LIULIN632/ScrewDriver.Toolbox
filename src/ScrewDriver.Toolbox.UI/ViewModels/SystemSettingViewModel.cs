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
        // 🎨 个性化
        AddItem("任务栏", "自定义任务栏行为和图标", "📌", "ms-settings:taskbar");
        AddItem("开始菜单", "开始菜单布局、推荐内容设置", "🏠", "ms-settings:personalization-start");
        AddItem("主题", "主题、颜色、壁纸和锁屏背景", "🎨", "ms-settings:themes");
        AddItem("桌面", "桌面图标、右键菜单和显示设置", "🖥️", "ms-settings:personalization");
        AddItem("锁屏", "锁屏背景和锁屏应用设置", "🔒", "ms-settings:lockscreen");

        // ⚡ 电源与设备
        AddItem("电源计划", "选择高性能/平衡/节能电源计划", "🔋", "powercfg.cpl");
        AddItem("显示器", "分辨率、缩放、多显示器设置", "🖥️", "ms-settings:display");
        AddItem("声音", "输出设备、麦克风、音量合成器", "🔊", "ms-settings:sound");
        AddItem("蓝牙", "管理蓝牙设备", "📶", "ms-settings:bluetooth");

        // 🌐 网络
        AddItem("WiFi 管理", "查看和管理 WiFi 网络", "📶", "ms-settings:network-wifi");
        AddItem("代理设置", "配置代理服务器", "🌐", "ms-settings:network-proxy");
        AddItem("网络重置", "重置网络适配器和协议栈", "🔄", "ms-settings:network-status");

        // 🛡️ 安全
        AddItem("Windows 安全中心", "防病毒、防火墙和网络保护", "🛡️", "windowsdefender://");
        AddItem("用户账户控制", "调整 UAC 通知级别", "🔐", "ms-settings:privacy");
        AddItem("Windows 更新", "检查更新、更新历史和高级选项", "🔄", "ms-settings:windowsupdate");

        // 💾 存储
        AddItem("存储感知", "自动清理临时文件和释放空间", "🗑️", "ms-settings:storage");
        AddItem("磁盘管理", "管理磁盘分区和驱动器号", "💽", "diskmgmt.msc");

        // 🛠 系统工具
        AddItem("系统还原", "创建或恢复系统还原点", "🔄", "systempropertiesprotection");
        AddItem("环境变量", "编辑系统环境变量", "📋", "sysdm.cpl");
        AddItem("高级系统设置", "性能、用户配置文件、启动和故障恢复", "⚙️", "systempropertiesadvanced");
        AddItem("远程桌面", "配置远程桌面连接", "🖥️", "sysdm.cpl");
        AddItem("系统信息", "查看系统硬件和软件环境详情", "ℹ️", "msinfo32");
        AddItem("设备管理器", "查看和管理硬件设备", "🔧", "devmgmt.msc");
        AddItem("服务", "管理 Windows 系统服务", "⚙️", "services.msc");
        AddItem("注册表编辑器", "查看和修改系统注册表", "📝", "regedit.exe");
        AddItem("任务计划程序", "管理计划任务和触发器", "⏰", "taskschd.msc");
        AddItem("事件查看器", "查看系统日志和应用程序日志", "📋", "eventvwr.msc");
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
