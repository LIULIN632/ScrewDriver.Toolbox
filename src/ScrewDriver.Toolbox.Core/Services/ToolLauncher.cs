using System.Diagnostics;
using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.Core.Services;

public class ToolLauncher : IToolLauncher
{
    public Task<int> LaunchAsync(string executablePath, string? arguments = null, string? workingDirectory = null)
        => Task.Run(() => Launch(executablePath, arguments, workingDirectory));

    public Task<int> LaunchElevatedAsync(string executablePath, string? arguments = null, string? workingDirectory = null)
        => Task.Run(() => LaunchElevated(executablePath, arguments, workingDirectory));

    public int Launch(string executablePath, string? arguments = null, string? workingDirectory = null)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(executablePath) ?? string.Empty,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();
            return process?.ExitCode ?? -1;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[ToolLauncher] Launch failed: {ex.Message}");
            return -1;
        }
    }

    public int LaunchElevated(string executablePath, string? arguments = null, string? workingDirectory = null)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(executablePath) ?? string.Empty,
                UseShellExecute = true,
                Verb = "runas",
                CreateNoWindow = false
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();
            return process?.ExitCode ?? -1;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return -1;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[ToolLauncher] Elevated launch failed: {ex.Message}");
            return -1;
        }
    }
}
