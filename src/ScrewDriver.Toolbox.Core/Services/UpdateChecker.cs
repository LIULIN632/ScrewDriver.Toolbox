using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace ScrewDriver.Toolbox.Core.Services;

public static class UpdateChecker
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>检查应用自身更新，返回最新版本号（null = 无法检查或无更新）</summary>
    public static async Task<string?> CheckAppVersionAsync(string currentVersion)
    {
        try
        {
            var url = "https://api.github.com/repos/LIULIN632/ScrewDriver.Toolbox/releases/latest";
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("ScrewDriverToolbox");
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var tag = doc.RootElement.GetProperty("tag_name").GetString();
            if (tag == null) return null;
            var latest = tag.TrimStart('v');
            return latest != currentVersion ? latest : null;
        }
        catch { return null; }
    }

    /// <summary>检查 winget 可更新的工具数量</summary>
    public static int CheckWingetUpdateCount()
    {
        try
        {
            var ids = GetUpdatableWingetIds();
            return ids.Count;
        }
        catch { return 0; }
    }
    public static List<string> GetUpdatableWingetIds()
    {
        var ids = new List<string>();
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = "upgrade --accept-source-agreements",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var headerFound = false;
            foreach (var line in lines)
            {
                if (!headerFound && line.StartsWith("ID"))
                {
                    headerFound = true;
                    continue;
                }
                if (headerFound && !string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        var id = parts[0];
                        if (!string.IsNullOrEmpty(id) && id.Contains('.'))
                            ids.Add(id);
                    }
                }
            }
        }
        catch
        {
            // winget unavailable or error — return empty list
        }
        return ids;
    }
}
