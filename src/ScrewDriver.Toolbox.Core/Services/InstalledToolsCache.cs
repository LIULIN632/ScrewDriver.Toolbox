using System.Security.Cryptography;
using System.Text;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public class InstalledToolsCache
{
    private static readonly Lazy<InstalledToolsCache> _instance = new(() => new InstalledToolsCache());
    public static InstalledToolsCache Instance => _instance.Value;

    private readonly object _lock = new();
    private List<ToolItem> _installedTools = new();
    public List<ToolItem> InstalledTools
    {
        get { lock (_lock) return new List<ToolItem>(_installedTools); }
    }

    public event Action? CacheUpdated;
    public bool IsInitialized { get; private set; }

    private readonly string _iconCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriverToolbox", "Icons");

    private InstalledToolsCache()
    {
        Directory.CreateDirectory(_iconCacheDir);
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            var allTools = ToolRegistry.GetAllTools();
            allTools.AddRange(ToolRegistry.GetCustomTools());

            foreach (var tool in allTools)
            {
                if (IsToolLaunchable(tool) && string.IsNullOrEmpty(tool.IconPath))
                    tool.IconPath = GetOrCreateIcon(tool);
            }

            var launchable = allTools
                .Where(IsToolLaunchable)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToList();

            lock (_lock)
            {
                _installedTools = launchable;
            }
        });

        IsInitialized = true;
        CacheUpdated?.Invoke();
    }

    public async Task RefreshAsync() => await InitializeAsync();

    private static bool IsToolLaunchable(ToolItem tool)
    {
        if (!string.IsNullOrEmpty(tool.LocalExePath) && File.Exists(tool.LocalExePath))
            return true;
        if (!string.IsNullOrEmpty(tool.LaunchPath) && File.Exists(tool.LaunchPath))
            return true;
        return false;
    }

    private string? GetOrCreateIcon(ToolItem tool)
    {
        string? exePath = null;
        if (!string.IsNullOrEmpty(tool.LocalExePath) && File.Exists(tool.LocalExePath))
            exePath = tool.LocalExePath;
        else if (!string.IsNullOrEmpty(tool.LaunchPath) && File.Exists(tool.LaunchPath))
            exePath = tool.LaunchPath;
        if (exePath == null) return null;

        var hash = ComputeHash(exePath);
        var cachedIcon = Path.Combine(_iconCacheDir, $"{hash}.png");
        if (File.Exists(cachedIcon)) return cachedIcon;

        var tempIcon = IconExtractor.ExtractIconToFile(exePath);
        if (tempIcon != null && File.Exists(tempIcon))
        {
            try
            {
                File.Copy(tempIcon, cachedIcon, true);
                return cachedIcon;
            }
            catch { return tempIcon; }
        }
        return tempIcon;
    }

    private static string ComputeHash(string filePath)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(filePath));
        return Convert.ToHexString(hash)[..16];
    }
}
