using System.Collections.ObjectModel;
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
    private List<ToolItem> _toolsFolderTools = new(); // Tools 中未注册的 exe
    private HashSet<string> _pinnedNames = new();
    private FileSystemWatcher? _watcher;

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

    /// <summary>Tools 目录中未注册仓库的额外工具</summary>
    public List<ToolItem> ToolsFolderTools
    {
        get
        {
            lock (_lock)
            {
                if (!IsInitialized)
                    _ = InitializeAsync();
                return new List<ToolItem>(_toolsFolderTools);
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
        catch { }
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
        catch { }
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
        await Task.Run(() => ScanTools());

        IsInitialized = true;
        CacheUpdated?.Invoke();

        // 注册文件监听，Tools 目录变动时自动刷新
        SetupFileWatcher();

        // Icons load lazily in background
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

    private void ScanTools()
    {
        var allTools = ToolRegistry.GetAllTools();
        allTools.AddRange(ToolRegistry.GetCustomTools());

        var toolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
        var toolsExeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var unknownExes = new List<string>();

        // 1. 扫描 Tools 目录一次，收集所有 exe
        if (Directory.Exists(toolsDir))
        {
            foreach (var exePath in Directory.GetFiles(toolsDir, "*.exe", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileNameWithoutExtension(exePath);
                toolsExeMap[fileName] = exePath;
            }
        }

        // 2. 一把匹配所有工具
        foreach (var tool in allTools)
        {
            if (!string.IsNullOrEmpty(tool.LocalExePath) && File.Exists(tool.LocalExePath))
                continue;

            // 尝试按工具名匹配
            var searchKey = tool.Name.Replace(" ", "").Replace("-", "").Replace(".", "");
            if (toolsExeMap.TryGetValue(searchKey, out var matchedPath))
            {
                tool.LocalExePath = matchedPath;
                toolsExeMap.Remove(searchKey);
            }
            else
            {
                // 模糊匹配：工具名包含 exe 名或反之
                var fuzzy = toolsExeMap.Keys.FirstOrDefault(k =>
                    k.Contains(searchKey, StringComparison.OrdinalIgnoreCase) ||
                    searchKey.Contains(k, StringComparison.OrdinalIgnoreCase));
                if (fuzzy != null)
                {
                    tool.LocalExePath = toolsExeMap[fuzzy];
                    toolsExeMap.Remove(fuzzy);
                }
            }
        }

        // 3. 未匹配到的 exe 作为"额外工具"保留
        unknownExes = toolsExeMap.Values.ToList();

        // 4. 筛选可启动的工具
        var launchable = allTools
            .Where(IsToolLaunchable)
            .Select(t => { t.IsPinned = IsPinned(t.Name); return t; })
            .OrderByDescending(t => t.IsPinned)
            .ThenBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToList();

        // 5. 为未匹配的 exe 创建临时 ToolItem
        var extraTools = unknownExes.Select(exe =>
        {
            var name = Path.GetFileNameWithoutExtension(exe);
            return new ToolItem
            {
                Name = name,
                LocalExePath = exe,
                Category = "其他工具",
                Description = $"Tools 目录: {name}",
                RiskLevel = "安全",
                IsInstalled = true
            };
        }).ToList();

        lock (_lock)
        {
            _installedTools = launchable;
            _toolsFolderTools = extraTools;
        }
    }

    private void SetupFileWatcher()
    {
        var toolsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
        if (!Directory.Exists(toolsDir)) return;

        try
        {
            _watcher?.Dispose();
            _watcher = new FileSystemWatcher(toolsDir, "*.exe")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            // 防抖：文件变动后等 1 秒再刷新，避免多次触发
            var timer = new System.Timers.Timer(1000) { AutoReset = false };
            _watcher.Changed += (_, _) => { timer.Stop(); timer.Start(); };
            _watcher.Created += (_, _) => { timer.Stop(); timer.Start(); };
            _watcher.Deleted += (_, _) => { timer.Stop(); timer.Start(); };
            timer.Elapsed += async (_, _) =>
            {
                await Task.Run(() => ScanTools());
                IsInitialized = true;
                CacheUpdated?.Invoke();
                timer.Dispose();
            };
        }
        catch { }
    }

    public async Task RefreshAsync() => await InitializeAsync();

    private static readonly HashSet<string> _systemExes = new(StringComparer.OrdinalIgnoreCase)
    {
        "cmd.exe", "notepad.exe", "certmgr.msc", "resmon.exe", "cleanmgr.exe",
        "ncpa.cpl", "msconfig.exe", "regedit.exe", "eventvwr.msc", "control.exe"
    };

    private static bool IsToolLaunchable(ToolItem tool)
    {
        if (!string.IsNullOrEmpty(tool.LocalExePath) && File.Exists(tool.LocalExePath))
            return true;

        var path = tool.LaunchPath;
        if (string.IsNullOrEmpty(path)) return false;

        if (path.StartsWith("ms-") || path.StartsWith("http") || path.StartsWith("https") ||
            path.StartsWith("scenario:") || path.StartsWith("windowsdefender:"))
            return true;

        if (path.StartsWith("cmd.exe ", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWith("notepad ", StringComparison.OrdinalIgnoreCase))
            return true;

        var fileName = Path.GetFileName(path);
        if (_systemExes.Contains(fileName))
            return true;

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
