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
    private HashSet<string> _pinnedNames = new();
    public List<ToolItem> InstalledTools
    {
        get
        {
            lock (_lock)
            {
                if (!IsInitialized)
                    _ = InitializeAsync();
                return new List<ToolItem>(_installedTools);
            }
        }
    }

    public event Action? CacheUpdated;
    public bool IsInitialized { get; private set; }

    private readonly string _iconCacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriverToolbox", "Icons");
    private readonly string _configDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriverToolbox");

    private InstalledToolsCache()
    {
        Directory.CreateDirectory(_iconCacheDir);
        Directory.CreateDirectory(_configDir);
        LoadPinnedNames();
    }

    public bool IsPinned(string toolName)
    {
        lock (_lock) return _pinnedNames.Contains(toolName);
    }

    public void TogglePin(string toolName)
    {
        lock (_lock)
        {
            if (!_pinnedNames.Remove(toolName))
                _pinnedNames.Add(toolName);
        }
        SavePinnedNames();
        RefreshPinnedState();
    }

    private void LoadPinnedNames()
    {
        try
        {
            var path = Path.Combine(_configDir, "pinned_tools.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var names = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                if (names != null)
                    _pinnedNames = new HashSet<string>(names);
            }
        }
        catch { /* ignore deserialize failures */ }
    }

    private void SavePinnedNames()
    {
        try
        {
            var path = Path.Combine(_configDir, "pinned_tools.json");
            lock (_lock)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_pinnedNames.ToList());
                File.WriteAllText(path, json);
            }
        }
        catch { /* ignore write failures */ }
    }

    private void RefreshPinnedState()
    {
        lock (_lock)
        {
            foreach (var tool in _installedTools)
                tool.IsPinned = _pinnedNames.Contains(tool.Name);
        }
        CacheUpdated?.Invoke();
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            var allTools = ToolRegistry.GetAllTools();
            allTools.AddRange(ToolRegistry.GetCustomTools());

            // 扫描 Tools 目录，匹配到对应工具
            var toolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
            if (Directory.Exists(toolsDir))
            {
                foreach (var tool in allTools)
                {
                    if (string.IsNullOrEmpty(tool.LocalExePath))
                    {
                        // 按工具名匹配 Tools 目录下的 exe
                        var exeName = tool.Name.Replace(" ", "").Replace("-", "") + ".exe";
                        var found = Directory.GetFiles(toolsDir, $"*{tool.Name}*", SearchOption.AllDirectories)
                            .FirstOrDefault(f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                        if (found != null)
                            tool.LocalExePath = found;
                    }
                }
            }

            var launchable = allTools
                .Where(IsToolLaunchable)
                .Select(t => { t.IsPinned = IsPinned(t.Name); return t; })
                .OrderByDescending(t => t.IsPinned)
                .ThenBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToList();

            lock (_lock)
            {
                _installedTools = launchable;
            }
        });

        IsInitialized = true;
        CacheUpdated?.Invoke();

        // Icons load lazily in background after tools are displayed
        _ = Task.Run(() =>
        {
            List<ToolItem> snapshot;
            lock (_lock) { snapshot = new List<ToolItem>(_installedTools); }

            var changed = false;
            foreach (var tool in snapshot)
            {
                if (string.IsNullOrEmpty(tool.IconPath))
                {
                    var icon = GetOrCreateIcon(tool);
                    if (icon != null) { tool.IconPath = icon; changed = true; }
                }
            }
            if (changed)
                CacheUpdated?.Invoke();
        });
    }

    public async Task RefreshAsync() => await InitializeAsync();

    private static readonly HashSet<string> _systemExes = new(StringComparer.OrdinalIgnoreCase)
    {
        "cmd.exe", "notepad.exe", "certmgr.msc", "resmon.exe", "cleanmgr.exe",
        "ncpa.cpl", "msconfig.exe", "regedit.exe", "eventvwr.msc", "control.exe"
    };

    private static bool IsToolLaunchable(ToolItem tool)
    {
        // 1. LocalExePath from drag-drop or local scan: check file exists
        if (!string.IsNullOrEmpty(tool.LocalExePath) && File.Exists(tool.LocalExePath))
            return true;

        var path = tool.LaunchPath;
        if (string.IsNullOrEmpty(path)) return false;

        // 2. Known URI schemes — always launchable without file check
        if (path.StartsWith("ms-") || path.StartsWith("http") || path.StartsWith("https") ||
            path.StartsWith("scenario:") || path.StartsWith("windowsdefender:"))
            return true;

        // 3. "cmd.exe /k ..." or "cmd.exe /c ..." — extract first token
        if (path.StartsWith("cmd.exe ", StringComparison.OrdinalIgnoreCase))
            return true; // cmd.exe always exists on Windows
        if (path.StartsWith("notepad ", StringComparison.OrdinalIgnoreCase))
            return true; // notepad.exe always exists on Windows

        // 4. Known system executables — always available on Windows
        var fileName = Path.GetFileName(path);
        if (_systemExes.Contains(fileName))
            return true;

        // 5. Fallback: check file existence on disk
        return File.Exists(path);
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
