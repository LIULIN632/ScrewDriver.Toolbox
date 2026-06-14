using System.Text.Json;

namespace ScrewDriver.Toolbox.Core.Services;

public class JsonConfigManager
{
    private readonly string _configDirectory;

    public JsonConfigManager(string configDirectory)
    {
        _configDirectory = configDirectory;
        Directory.CreateDirectory(_configDirectory);
    }

    public T? Load<T>(string configName) where T : class
    {
        var path = GetPath(configName);
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Save<T>(string configName, T config) where T : class
    {
        var path = GetPath(configName);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(path, json);
    }

    public bool Exists(string configName) => File.Exists(GetPath(configName));

    public void Delete(string configName)
    {
        var path = GetPath(configName);
        if (File.Exists(path)) File.Delete(path);
    }

    private string GetPath(string configName)
        => Path.Combine(_configDirectory,
            configName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? configName
                : $"{configName}.json");
}
