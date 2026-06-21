using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class TaskbarDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        // ================================================================
        // 任务栏
        // ================================================================
        ("taskbar-combine", "任务栏不合并", "任务栏按钮不合并，始终显示标签",
         "任务栏", RiskLevel.Optional, "仅影响任务栏外观，可随时恢复", "将 TaskbarGlomLevel 改回 1 即可合并",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarGlomLevel", 2, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-widgets", "关闭小组件", "禁用任务栏小组件按钮和资讯推送",
         "任务栏", RiskLevel.Recommended, "关闭后无法使用天气、资讯等小组件功能", "将 AllowNewsAndInterests 改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-search-highlights", "关闭搜索亮点", "禁用任务栏搜索框的每日亮点图标和推荐",
         "任务栏", RiskLevel.Recommended, "仅关闭搜索框的图形亮点，不影响搜索功能", "将 IsDynamicSearchBoxEnabled 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("hide-taskbar-search", "隐藏任务栏搜索框", "完全隐藏任务栏上的搜索框",
         "任务栏", RiskLevel.Optional, "隐藏后可通过开始菜单直接输入进行搜索", "将 SearchboxTaskbarMode 改回 2 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("hide-taskview", "隐藏任务视图按钮", "隐藏任务栏上的「任务视图」按钮",
         "任务栏", RiskLevel.Optional, "仅移除按钮，Win+Tab 快捷键仍可使用", "将 ShowTaskViewButton 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        RegToggle("taskbar-seconds", "任务栏显示秒", "系统时钟显示秒数",
            "任务栏", RiskLevel.Optional, "显示秒数会略微增加 CPU 占用（时钟每秒刷新）", "将 ShowSecondsInSystemClock 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSecondsInSystemClock"),

        RegToggle("taskbar-alignment-left", "任务栏左对齐", "将开始按钮和任务栏图标靠左对齐（恢复 Win10 风格）",
            "任务栏", RiskLevel.Optional, "仅改变图标对齐方式，可随时恢复", "将 TaskbarAl 改回 1 即可恢复居中",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0, 1),

        RegToggle("taskbar-badge", "任务栏图标徽标", "在任务栏图标上显示未读计数等徽标",
            "任务栏", RiskLevel.Optional, "仅影响图标徽标显示，不影响通知功能", "将 TaskbarBadges 改回 0 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarBadges"),

        RegToggle("taskbar-tray-icons-all", "显示所有通知区域图标", "任务栏通知区域始终显示所有图标",
            "任务栏", RiskLevel.Optional, "显示所有图标占用更多任务栏空间", "将 EnableAutoTray 改回 1 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "EnableAutoTray", 0, 1),

        ("disable-taskbar-flashing", "关闭任务栏闪烁", "程序需要关注时不再闪烁任务栏按钮（改为静止高亮）",
            "任务栏", RiskLevel.Optional, "关闭闪烁可能导致错过重要通知", "将 ForegroundFlashCount 改回 7 并删除 ForegroundLockTimeout",
            RegistryHive.LocalMachine, "", "", 0, 0, RegistryValueKind.DWord,
            "PowerShell",
            "Set-ItemProperty 'HKCU:\\Control Panel\\Desktop' -Name ForegroundFlashCount -Value 0 -Type DWord; Set-ItemProperty 'HKCU:\\Control Panel\\Desktop' -Name ForegroundLockTimeout -Value 0 -Type DWord",
            "Set-ItemProperty 'HKCU:\\Control Panel\\Desktop' -Name ForegroundFlashCount -Value 7 -Type DWord; Remove-ItemProperty 'HKCU:\\Control Panel\\Desktop' -Name ForegroundLockTimeout -Force -ErrorAction SilentlyContinue"),

        ("disable-taskbar-thumbnails", "关闭缩略图预览", "鼠标悬停任务栏图标时不显示窗口缩略图预览",
            "任务栏", RiskLevel.Optional, "仅关闭缩略图，不影响任务栏基本功能", "将 NumThumbnails 键删除即可恢复",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband", "NumThumbnails", 0, null, RegistryValueKind.DWord,
            "Registry", null, null),

        RegToggle("disable-news-interests", "关闭新闻与兴趣", "关闭任务栏新闻和兴趣资讯推送（Win10 风格）",
            "任务栏", RiskLevel.Recommended, "仅关闭资讯推送，任务栏其他功能不受影响", "将 ShellFeedsTaskbarViewMode 改回 2 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 0, 2),

        RegToggle("taskbar-search-icon", "仅显示搜索图标", "任务栏搜索改为仅显示图标（不显示完整搜索框）",
            "任务栏", RiskLevel.Optional, "隐藏完整搜索框可释放任务栏空间", "将 SearchboxTaskbarMode 改回 2 即可恢复搜索框",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, 2),

        // ================================================================
        // 开始菜单
        // ================================================================
        ("disable-cortana", "禁用 Cortana", "彻底禁用 Cortana 语音助手",
         "开始菜单", RiskLevel.Recommended, "仅禁用语音助手，不影响搜索功能", "将 AllowCortana 改回 1 即可恢复",
         RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("disable-suggestions-in-start", "开始菜单建议", "移除开始菜单中的推荐应用和内容建议",
         "开始菜单", RiskLevel.Recommended, "仅移除推荐区域，不影响已固定的应用", "将 Start_IrisRecommendations 改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        RegToggle("start-hide-recent-items", "隐藏最近打开的文件", "不在开始菜单中显示最近打开的文件列表",
            "开始菜单", RiskLevel.Optional, "仅影响开始菜单文件列表，不影响实际文件", "将 Start_TrackDocs 改回 1 即可恢复",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, 1),

        RegToggle("start-hide-recently-added", "隐藏最近添加的应用", "不在开始菜单中显示最近安装的应用",
            "开始菜单", RiskLevel.Optional, "仅影响开始菜单应用列表", "将 Start_TrackProgs 改回 1 即可恢复",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, 1),

        RegToggle("start-show-most-used", "显示最常用应用", "在开始菜单中显示最常使用的应用列表",
            "开始菜单", RiskLevel.Recommended, "仅影响开始菜单中「最常用」区域，便于快速启动常用应用", "将 Start_ShowMostUsedApps 改回 0 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_ShowMostUsedApps"),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["taskbar-combine"] = RecommendedAction.Enable,
        ["disable-widgets"] = RecommendedAction.Enable,
        ["disable-search-highlights"] = RecommendedAction.Enable,
        ["hide-taskbar-search"] = RecommendedAction.Enable,
        ["hide-taskview"] = RecommendedAction.Enable,
        ["taskbar-seconds"] = RecommendedAction.None,
        ["taskbar-alignment-left"] = RecommendedAction.None,
        ["taskbar-badge"] = RecommendedAction.Enable,
        ["taskbar-tray-icons-all"] = RecommendedAction.None,
        ["disable-taskbar-flashing"] = RecommendedAction.Disable,
        ["disable-taskbar-thumbnails"] = RecommendedAction.Disable,
        ["disable-news-interests"] = RecommendedAction.Enable,
        ["taskbar-search-icon"] = RecommendedAction.Enable,
        ["disable-cortana"] = RecommendedAction.Enable,
        ["disable-suggestions-in-start"] = RecommendedAction.Enable,
        ["start-hide-recent-items"] = RecommendedAction.None,
        ["start-hide-recently-added"] = RecommendedAction.None,
        ["start-show-most-used"] = RecommendedAction.Enable,
    };
}
