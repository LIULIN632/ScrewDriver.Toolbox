using System.Text.Json;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class PresetStore
{
    private static readonly string PresetDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriver", "Presets");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    static PresetStore()
    {
        Directory.CreateDirectory(PresetDir);
    }

    public static List<PresetDefinition> LoadPresets(Func<List<PresetDefinition>> seedFactory)
    {
        var files = Directory.GetFiles(PresetDir, "*.json");
        if (files.Length == 0)
        {
            var defaults = seedFactory();
            foreach (var def in defaults)
                SavePreset(def);
            return defaults;
        }

        var presets = new List<PresetDefinition>();
        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var preset = JsonSerializer.Deserialize<PresetDefinition>(json);
                if (preset != null)
                    presets.Add(preset);
            }
            catch { }
        }
        return presets;
    }

    public static void SavePreset(PresetDefinition preset)
    {
        var path = Path.Combine(PresetDir, $"{preset.Id}.json");
        var json = JsonSerializer.Serialize(preset, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static void DeletePreset(string id)
    {
        var path = Path.Combine(PresetDir, $"{id}.json");
        if (File.Exists(path)) File.Delete(path);
    }

    public static bool Exists(string id)
        => File.Exists(Path.Combine(PresetDir, $"{id}.json"));

    public static void ResetToDefaults(Func<List<PresetDefinition>> seedFactory)
    {
        foreach (var file in Directory.GetFiles(PresetDir, "*.json"))
        {
            try { File.Delete(file); } catch { }
        }
        var defaults = seedFactory();
        foreach (var def in defaults)
            SavePreset(def);
    }
}
