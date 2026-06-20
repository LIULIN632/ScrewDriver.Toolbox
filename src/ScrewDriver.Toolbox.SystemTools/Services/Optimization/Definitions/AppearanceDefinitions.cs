using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class AppearanceDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        // ================================================================
        // 个性化
        // ================================================================
        ("disable-lockscreen", "禁用锁屏界面", "跳过锁屏画面直接进入登录界面",
         "个性化", RiskLevel.Recommended, "仅跳过锁屏画面，不影响锁屏安全性", "删除 NoLockScreen 键值即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Personalization", "NoLockScreen", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-this-pc", "桌面显示「此电脑」", "在桌面上显示「此电脑」图标",
         "个性化", RiskLevel.Recommended, "仅添加桌面图标，不影响系统功能", "将对应键值改回 1 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-recycle-bin", "桌面显示「回收站」", "在桌面上显示「回收站」图标",
         "个性化", RiskLevel.Recommended, "仅添加桌面图标，不影响系统功能", "将对应键值改回 1 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-login-password", "免密自动登录", "⚠️ 密码将明文存储在注册表中！启用后开机跳过密码直接进入桌面",
         "个性化", RiskLevel.Dangerous, "⚠️ 密码明文保存在注册表，任何人可读取。仅限单用户家庭环境，离开电脑请锁定（Win+L）。启用后需在「netplwiz」中取消「要使用本计算机，用户必须输入用户名和密码」",
         "将 AutoAdminLogon 改回 0 并清除 DefaultPassword 即可恢复密码登录",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "AutoAdminLogon", "1", "0", RegistryValueKind.String,
         "PowerShell",
         "Set-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon' -Name AutoAdminLogon -Value '1'",
         "Set-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon' -Name AutoAdminLogon -Value '0'; Remove-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon' -Name DefaultPassword -Force -ErrorAction SilentlyContinue"),

        ("disable-animation", "关闭窗口动画", "关闭窗口最小化/最大化动画，提升操作流畅度",
         "个性化", RiskLevel.Recommended, "仅影响视觉效果，无副作用，对流畅度提升明显", "将对应键值改回 0 即可恢复动画",
         RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-transparency", "关闭透明效果", "关闭窗口标题栏和任务栏的透明/毛玻璃效果",
         "个性化", RiskLevel.Optional, "关闭透明效果可略微降低 GPU 占用", "将 EnableTransparency 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        RegToggle("dark-mode-apps", "深色模式（应用）", "将应用切换为深色主题",
            "个性化", RiskLevel.Recommended, "仅影响应用颜色主题，可随时切换", "将 AppsUseLightTheme 改回 1 即可恢复浅色",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, 1),

        RegToggle("dark-mode-system", "深色模式（系统）", "将系统界面（任务栏/开始菜单等）切换为深色主题",
            "个性化", RiskLevel.Recommended, "仅影响系统界面颜色，可随时切换", "将 SystemUsesLightTheme 改回 1 即可恢复浅色",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, 1),

        RegToggle("accent-color-title-bar", "标题栏着色", "在窗口标题栏和边框上显示主题色",
            "个性化", RiskLevel.Optional, "仅改变标题栏外观，不影响窗口功能", "将 DWM ColorPrevalence 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\DWM", "ColorPrevalence"),

        RegToggle("accent-color-start-taskbar", "开始菜单/任务栏着色", "在开始菜单和任务栏上显示主题色",
            "个性化", RiskLevel.Optional, "仅改变开始菜单和任务栏外观", "将 ColorPrevalence 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "ColorPrevalence"),

        ("drag-full-window", "拖动时显示窗口内容", "拖动窗口时显示完整窗口内容而非仅显示轮廓框",
            "个性化", RiskLevel.Optional, "仅改变拖动时的视觉效果，对老显卡可能略有性能影响", "将 DragFullWindows 改回 0 即可恢复仅显示轮廓",
            RegistryHive.CurrentUser, @"Control Panel\Desktop", "DragFullWindows", "1", "0", RegistryValueKind.String,
            "Registry", null, null),

        ("font-smoothing", "字体平滑（ClearType）", "启用 ClearType 字体平滑技术，提升文字可读性",
            "个性化", RiskLevel.Recommended, "大多数显示器建议开启，部分像素排列特殊的显示器可能适得其反", "将 FontSmoothing 改回 0 即可关闭",
            RegistryHive.CurrentUser, @"Control Panel\Desktop", "FontSmoothing", "2", "0", RegistryValueKind.String,
            "Registry", null, null),

        ("fast-menu", "加速菜单显示", "将菜单弹出延迟设为 0ms，操作响应更及时",
            "个性化", RiskLevel.Recommended, "仅消除菜单动画延迟，无副作用", "将 MenuShowDelay 改回 400 即可恢复默认延迟",
            RegistryHive.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", "0", "400", RegistryValueKind.String,
            "Registry", null, null),

        ("snap-windows", "窗口贴靠功能", "将窗口拖拽到屏幕边缘时自动调整大小和位置",
            "个性化", RiskLevel.Recommended, "关闭后拖拽窗口不会自动贴靠排列，可随时恢复", "将 WindowArrangementActive 改回 1 即可恢复",
            RegistryHive.CurrentUser, @"Control Panel\Desktop", "WindowArrangementActive", "0", "1", RegistryValueKind.String,
            "Registry", null, null),

        RegToggle("wallpaper-compression", "禁用壁纸压缩", "不再对 JPEG 壁纸进行有损压缩，保持原始画质",
            "个性化", RiskLevel.Optional, "仅影响壁纸画质，禁用后壁纸文件可能占用更多内存", "将 JPEGImportQuality 删除即可恢复默认压缩",
            RegistryHive.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality", 100, null),

        // ================================================================
        // 桌面
        // ================================================================
        RegToggle("show-network-desktop", "桌面显示「网络」", "在桌面上显示「网络」图标",
            "桌面", RiskLevel.Optional, "仅添加桌面图标，不影响网络功能", "将对应键值改回 1 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", 0, 1),

        RegToggle("show-control-panel-desktop", "桌面显示「控制面板」", "在桌面上显示「控制面板」图标",
            "桌面", RiskLevel.Optional, "仅添加桌面图标，不影响系统功能", "将对应键值改回 1 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", 0, 1),

        RegToggle("show-user-folder-desktop", "桌面显示「用户文件夹」", "在桌面上显示当前用户的个人文件夹图标",
            "桌面", RiskLevel.Optional, "仅添加桌面图标，不影响文件夹内容", "将对应键值改回 1 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{59031a47-3f72-44a7-89c5-5595fe6b30ee}", 0, 1),

        RegToggle("hide-desktop-icons", "隐藏所有桌面图标", "临时隐藏桌面所有图标（右键桌面 → 查看 → 显示桌面图标 也可切换）",
            "桌面", RiskLevel.Optional, "仅隐藏图标，文件和文件夹不受影响", "将 HideIcons 改回 0 即可显示",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideIcons"),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["disable-lockscreen"] = RecommendedAction.Enable,
        ["show-this-pc"] = RecommendedAction.Enable,
        ["show-recycle-bin"] = RecommendedAction.Enable,
        ["disable-login-password"] = RecommendedAction.Disable,
        ["disable-animation"] = RecommendedAction.Enable,
        ["disable-transparency"] = RecommendedAction.Enable,
        ["dark-mode-apps"] = RecommendedAction.None,
        ["dark-mode-system"] = RecommendedAction.None,
        ["accent-color-title-bar"] = RecommendedAction.None,
        ["accent-color-start-taskbar"] = RecommendedAction.None,
        ["drag-full-window"] = RecommendedAction.Enable,
        ["font-smoothing"] = RecommendedAction.Enable,
        ["fast-menu"] = RecommendedAction.Enable,
        ["snap-windows"] = RecommendedAction.Enable,
        ["wallpaper-compression"] = RecommendedAction.Enable,
        ["show-network-desktop"] = RecommendedAction.None,
        ["show-control-panel-desktop"] = RecommendedAction.None,
        ["show-user-folder-desktop"] = RecommendedAction.None,
        ["hide-desktop-icons"] = RecommendedAction.None,
    };
}
