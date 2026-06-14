using ScrewDriver.Toolbox.Models;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.Services;

public class ToolCatalogService : IToolCatalogService
{
    private readonly List<ToolModel> _tools = [];

    public ToolCatalogService()
    {
        InitializeTools();
    }

    private void InitializeTools()
    {
        // 系统工具
        _tools.Add(new ToolModel { Id = "everything", Name = "Everything", Description = "极速文件搜索工具", Category = "系统工具", OfficialUrl = "https://www.voidtools.com" });
        _tools.Add(new ToolModel { Id = "wiztree", Name = "WizTree", Description = "磁盘空间分析工具", Category = "系统工具" });
        _tools.Add(new ToolModel { Id = "dismpp", Name = "DISM++", Description = "系统优化与清理工具", Category = "系统工具" });
        _tools.Add(new ToolModel { Id = "autoruns", Name = "Autoruns", Description = "启动项管理工具", Category = "系统工具" });
        _tools.Add(new ToolModel { Id = "geek", Name = "Geek Uninstaller", Description = "软件彻底卸载工具", Category = "系统工具" });

        // 硬件工具
        _tools.Add(new ToolModel { Id = "cpuz", Name = "CPU-Z", Description = "CPU 详细信息检测", Category = "硬件工具" });
        _tools.Add(new ToolModel { Id = "gpuz", Name = "GPU-Z", Description = "显卡详细信息检测", Category = "硬件工具" });
        _tools.Add(new ToolModel { Id = "hwinfo", Name = "HWiNFO", Description = "全面硬件信息检测", Category = "硬件工具" });
        _tools.Add(new ToolModel { Id = "crystaldiskinfo", Name = "CrystalDiskInfo", Description = "硬盘健康检测", Category = "硬件工具" });
        _tools.Add(new ToolModel { Id = "crystaldiskmark", Name = "CrystalDiskMark", Description = "硬盘性能测试", Category = "硬件工具" });

        // 效率工具
        _tools.Add(new ToolModel { Id = "snipaste", Name = "Snipaste", Description = "截图贴图工具", Category = "效率工具" });
        _tools.Add(new ToolModel { Id = "screentogif", Name = "ScreenToGif", Description = "屏幕 GIF 录制", Category = "效率工具" });
        _tools.Add(new ToolModel { Id = "7zip", Name = "7-Zip", Description = "开源压缩解压工具", Category = "效率工具" });
        _tools.Add(new ToolModel { Id = "ditto", Name = "Ditto", Description = "剪贴板增强管理", Category = "效率工具" });

        // 品牌工具
        _tools.Add(new ToolModel { Id = "llt", Name = "Lenovo Legion Toolkit", Description = "联想拯救者开源控制工具", Category = "品牌工具" });
        _tools.Add(new ToolModel { Id = "ghelper", Name = "G-Helper", Description = "华硕笔记本开源控制工具", Category = "品牌工具" });
        _tools.Add(new ToolModel { Id = "fancontrol", Name = "Fan Control", Description = "通用风扇精准控制", Category = "品牌工具" });
        _tools.Add(new ToolModel { Id = "openrgb", Name = "OpenRGB", Description = "全品牌RGB灯光统一控制", Category = "品牌工具" });
        _tools.Add(new ToolModel { Id = "throttlestop", Name = "ThrottleStop", Description = "Intel处理器功耗/降压调节", Category = "品牌工具" });

        // 开发工具
        _tools.Add(new ToolModel { Id = "vscode", Name = "VS Code", Description = "轻量级代码编辑器", Category = "开发工具" });
        _tools.Add(new ToolModel { Id = "git", Name = "Git", Description = "分布式版本控制", Category = "开发工具" });
        _tools.Add(new ToolModel { Id = "winmerge", Name = "WinMerge", Description = "文件差异比较工具", Category = "开发工具" });

        // 网络工具
        _tools.Add(new ToolModel { Id = "wireshark", Name = "Wireshark", Description = "网络封包分析工具", Category = "网络工具" });
        _tools.Add(new ToolModel { Id = "speedtest", Name = "SpeedTest", Description = "网络测速工具", Category = "网络工具" });
    }

    public ObservableCollection<string> GetCategories() =>
    [
        "系统工具", "硬件工具", "效率工具", "品牌工具", "开发工具", "网络工具"
    ];

    public ObservableCollection<ToolModel> GetToolsByCategory(string category) =>
        new(_tools.Where(t => t.Category == category));

    public ObservableCollection<ToolModel> SearchTools(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new ObservableCollection<ToolModel>(_tools);

        var q = query.ToLower();
        return new ObservableCollection<ToolModel>(
            _tools.Where(t => t.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                              t.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
    }

    public ToolModel? GetToolById(string id) =>
        _tools.FirstOrDefault(t => t.Id == id);
}
