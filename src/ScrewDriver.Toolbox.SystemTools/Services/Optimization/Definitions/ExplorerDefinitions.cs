using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using static ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions.DefinitionHelper;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (
    string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal,
    RegistryValueKind Kind, string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class ExplorerDefinitions
{
    public static readonly List<SettingDef> Definitions = new()
    {
        ("show-extensions", "显示文件扩展名", "在资源管理器中显示已知文件类型的扩展名",
         "资源管理器", RiskLevel.Recommended, "仅影响文件名的显示方式，不影响文件本身", "将 HideFileExt 值改回 1 即可隐藏扩展名",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-hidden-files", "显示隐藏文件", "在资源管理器中显示隐藏文件和文件夹",
         "资源管理器", RiskLevel.Optional, "隐藏文件通常包含系统配置，误删可能导致问题", "将 Hidden 值改回 2 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, 2, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-system-files", "显示系统文件", "取消隐藏受保护的操作系统文件",
         "资源管理器", RiskLevel.Optional, "风险在于误删而非显示本身，请勿删除不认识的系统文件", "将 ShowSuperHidden 改回 0 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("expand-to-current-folder", "展开到当前文件夹", "导航窗格自动展开到当前打开的文件夹",
         "资源管理器", RiskLevel.Recommended, "仅改变导航窗格行为，不影响文件数据", "将 NavPaneExpandToCurrentFolder 改回 0 即可关闭",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "NavPaneExpandToCurrentFolder", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        ("classic-context", "经典右键菜单", "恢复 Windows 10 风格的完整右键菜单（跳过「显示更多选项」）",
         "资源管理器", RiskLevel.Optional, "修改注册表 CLSID 可能影响右键菜单稳定性", "删除该 CLSID 键即可恢复 Win11 默认菜单",
         RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", null, RegistryValueKind.String,
         "Registry", null, null),

        ("restore-photo-viewer", "恢复照片查看器", "恢复 Windows 经典照片查看器为默认图片浏览器",
         "资源管理器", RiskLevel.Optional, "照片查看器不支持部分新格式（如 HEIC/WebP）", "删除 PhotoViewer 注册表项即可还原",
         RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows Photo Viewer\Capabilities\FileAssociations", ".jpg", "PhotoViewer.FileAssoc.Tiff", null, RegistryValueKind.String,
         "Registry", null, null),

        ("disable-quick-access-history", "关闭快速访问历史", "不再在快速访问中显示最近使用的文件和常用文件夹",
         "资源管理器", RiskLevel.Optional, "关闭后快速访问只剩固定文件夹，可能降低日常效率", "将对应键值改回 1 即可恢复",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, 1, RegistryValueKind.DWord,
         "Registry", null, null),

        ("show-full-path-in-title", "标题栏显示完整路径", "在资源管理器标题栏显示当前文件夹的完整路径",
         "资源管理器", RiskLevel.Recommended, "仅改变标题栏显示，便于定位当前目录位置", "将 FullPath 改回 0 即可隐藏",
         RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\CabinetState", "FullPath", 1, 0, RegistryValueKind.DWord,
         "Registry", null, null),

        RegToggle("show-status-bar", "显示状态栏", "在资源管理器底部显示状态栏",
            "资源管理器", RiskLevel.Recommended, "仅影响资源管理器窗口外观", "将 ShowStatusBar 改回 0 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowStatusBar"),

        RegToggle("open-to-pc", "打开此电脑", "资源管理器默认打开「此电脑」而非「快速访问」",
            "资源管理器", RiskLevel.Recommended, "仅改变默认启动位置，不影响其他功能", "将 LaunchTo 改回 2 即可恢复快速访问",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, 2),

        RegToggle("hide-common-folders", "隐藏常用文件夹", "不在快速访问中显示常用文件夹",
            "资源管理器", RiskLevel.Optional, "关闭后快速访问仅显示固定项，日常导航效率可能降低", "将 ShowFrequent 改回 1 即可恢复",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, 1),

        RegToggle("nav-pane-all-folders", "导航窗格展开所有文件夹", "导航窗格显示完整的文件夹树结构",
            "资源管理器", RiskLevel.Optional, "仅改变导航窗格外观，可能使列表变长不便浏览", "将 NavPaneShowAllFolders 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "NavPaneShowAllFolders"),

        RegToggle("show-libraries", "显示库", "在导航窗格中显示「库」节点",
            "资源管理器", RiskLevel.Recommended, "仅添加导航节点，不影响文件数据", "将 NavPaneShowLibraries 改回 0 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "NavPaneShowLibraries"),

        RegToggle("compact-mode", "紧凑模式", "减小资源管理器功能区高度，适合小屏幕",
            "资源管理器", RiskLevel.Optional, "仅改变间距，不影响功能", "将 UseCompactMode 改回 0 即可恢复",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode"),

        RegToggle("check-box-select", "复选框选择", "在文件/文件夹前显示复选框，方便触屏或多选操作",
            "资源管理器", RiskLevel.Optional, "仅改变选择方式，不影响文件数据", "将 AutoCheckSelect 改回 0 即可隐藏",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "AutoCheckSelect"),

        RegToggle("show-drive-letters-before", "驱动器号在前", "驱动器号显示在卷标前面（如 C: 系统）",
            "资源管理器", RiskLevel.Optional, "仅改变显示格式，不影响硬盘数据", "将 ShowDriveLettersFirst 改回 2 即可恢复卷标在前",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowDriveLettersFirst", 1, 2),

        RegToggle("show-encrypted-compressed-color", "加密/压缩文件着色", "用绿色/蓝色文字显示加密和压缩的 NTFS 文件",
            "资源管理器", RiskLevel.Recommended, "仅改变文字颜色，不影响文件内容", "将 ShowEncryptCompressedColor 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowEncryptCompressedColor"),

        RegToggle("launch-folder-windows-separate", "独立进程打开文件夹", "每个资源管理器窗口运行在独立进程中",
            "资源管理器", RiskLevel.Optional, "启用后各窗口独立运行，一个崩溃不影响其他窗口，但略微增加内存占用", "将 SeparateProcess 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SeparateProcess"),

        RegToggle("hide-sync-provider", "隐藏同步提供商", "不显示 OneDrive 等云同步服务商图标",
            "资源管理器", RiskLevel.Optional, "仅隐藏图标，不影响同步功能", "将 HideSyncProviderNotifications 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideSyncProviderNotifications"),

        RegToggle("disable-thumbnails", "显示图标不显示缩略图", "文件夹中的图片/视频仅显示图标而非预览缩略图",
            "资源管理器", RiskLevel.Optional, "关闭缩略图可提升文件夹浏览速度，但无法快速预览图片内容", "将 IconsOnly 改回 0 即可恢复缩略图",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "IconsOnly"),

        RegToggle("disable-thumbnail-cache", "关闭缩略图缓存", "不再生成 thumbs.db 缩略图缓存文件",
            "资源管理器", RiskLevel.Optional, "关闭后可减少磁盘碎片和隐藏文件，但文件夹缩略图加载变慢", "将 DisableThumbnailCache 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DisableThumbnailCache"),

        ("auto-complete-path", "地址栏自动补全", "在资源管理器地址栏输入时自动弹出路径建议",
            "资源管理器", RiskLevel.Recommended, "仅改变输入行为，便于快速导航到深层目录", "将 AutoSuggest 值改为 no 即可关闭",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoComplete", "AutoSuggest", "yes", "no", RegistryValueKind.String,
            "Registry", null, null),

        RegToggle("show-preview-pane", "默认显示预览窗格", "资源管理器默认显示右侧预览窗格",
            "资源管理器", RiskLevel.Optional, "预览窗格占用窗口空间，但方便快速查看文件内容", "将 ReadingPaneOn 改回 0 即可",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Modules\GlobalSettings\Sizer", "ReadingPaneOn"),
    };

    public static readonly Dictionary<string, RecommendedAction> Recommendations = new()
    {
        ["show-extensions"] = RecommendedAction.Enable,
        ["show-hidden-files"] = RecommendedAction.Enable,
        ["show-system-files"] = RecommendedAction.Disable,
        ["expand-to-current-folder"] = RecommendedAction.Enable,
        ["classic-context"] = RecommendedAction.Enable,
        ["restore-photo-viewer"] = RecommendedAction.Enable,
        ["disable-quick-access-history"] = RecommendedAction.None,
        ["show-full-path-in-title"] = RecommendedAction.Enable,
        ["show-status-bar"] = RecommendedAction.Enable,
        ["open-to-pc"] = RecommendedAction.Enable,
        ["hide-common-folders"] = RecommendedAction.None,
        ["nav-pane-all-folders"] = RecommendedAction.None,
        ["show-libraries"] = RecommendedAction.Enable,
        ["compact-mode"] = RecommendedAction.None,
        ["check-box-select"] = RecommendedAction.None,
        ["show-drive-letters-before"] = RecommendedAction.Enable,
        ["show-encrypted-compressed-color"] = RecommendedAction.Enable,
        ["launch-folder-windows-separate"] = RecommendedAction.None,
        ["hide-sync-provider"] = RecommendedAction.Enable,
        ["disable-thumbnails"] = RecommendedAction.Disable,
        ["disable-thumbnail-cache"] = RecommendedAction.Disable,
        ["auto-complete-path"] = RecommendedAction.Enable,
        ["show-preview-pane"] = RecommendedAction.None,
    };
}
