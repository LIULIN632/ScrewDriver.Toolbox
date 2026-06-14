using System.Diagnostics;

namespace ScrewDriver.Toolbox.Core.Services;

public static class UpdateChecker
{
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
