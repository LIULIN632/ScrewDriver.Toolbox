using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class ConfigManager
{
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriver", "Configs");

    static ConfigManager()
    {
        try { Directory.CreateDirectory(ConfigDir); } catch { }
    }

    /// <summary>导出当前所有系统设置到JSON文件</summary>
    public static void ExportConfig(string filePath, List<SettingSnapshot> snapshots)
    {
        var config = new Dictionary<string, object>();
        foreach (var snapshot in snapshots)
        {
            config[snapshot.Key] = new { Value = snapshot.CurrentValue, snapshot.ValueName };
        }

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    /// <summary>从JSON文件导入配置</summary>
    public static Dictionary<string, bool> LoadConfig(string filePath)
    {
        if (!File.Exists(filePath)) return new();

        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (config == null) return new();

        var result = new Dictionary<string, bool>();
        foreach (var (key, value) in config)
        {
            var enabled = value.GetProperty("Value").GetBoolean();
            result[key] = enabled;
        }
        return result;
    }

    /// <summary>列出已保存的配置文件</summary>
    public static List<string> GetSavedConfigs()
    {
        if (!Directory.Exists(ConfigDir)) return new();
        return Directory.GetFiles(ConfigDir, "*.json")
            .Select(f => Path.GetFileName(f))
            .Where(n => n != null)
            .Select(n => n!)
            .ToList();
    }
}
