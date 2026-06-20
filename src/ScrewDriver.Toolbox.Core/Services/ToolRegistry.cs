using System.Text.Json;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class ToolRegistry
{
    private static List<ToolItem>? _allTools;
    private static List<ToolItem>? _customTools;
    private static readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);

    public static List<string> Categories { get; } = new()
    {
        "系统工具", "CPU工具", "主板工具", "内存工具",
        "显卡工具", "硬盘工具", "屏幕工具", "外设工具",
        "安全工具", "联想工具", "华硕工具", "惠普工具", "戴尔工具", "微星工具", "机械革命", "通用工具", "启动与镜像",
        "游戏工具", "烤机工具", "音视频播放器", "音视频处理工具", "图像与设计工具", "其他工具"
    };

    public static List<ToolItem> GetAllTools()
    {
        if (_allTools == null)
        {
            _allTools = LoadBuiltInTools();
        }
        var all = new List<ToolItem>(_allTools);
        all.AddRange(GetCustomTools());
        return all;
    }

    private static List<ToolItem> LoadBuiltInTools()
    {
        var tools = new List<ToolItem>();
        var toolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Tools");
        if (!Directory.Exists(toolsDir)) return tools;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (var file in Directory.GetFiles(toolsDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var items = JsonSerializer.Deserialize<List<ToolItem>>(json, options);
                if (items != null) tools.AddRange(items);
            }
            catch { /* skip malformed files */ }
        }
        return tools;
    }

    public static List<ToolItem> GetCustomTools()
    {
        if (_customTools == null)
            _customTools = _config.Load<List<ToolItem>>("custom-tools") ?? new List<ToolItem>();
        return _customTools;
    }

    public static void AddCustomTool(ToolItem tool)
    {
        var tools = GetCustomTools();
        tools.Add(tool);
        SaveCustomTools();
    }

    public static void RemoveCustomTool(ToolItem tool)
    {
        var tools = GetCustomTools();
        tools.Remove(tool);
        SaveCustomTools();
    }

    private static void SaveCustomTools()
    {
        _config.Save("custom-tools", _customTools!);
    }
}
