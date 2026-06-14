using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

[SupportedOSPlatform("windows")]
public static class InstallationDetector
{
    public static bool IsToolInstalled(ToolItem tool)
    {
        if (!string.IsNullOrEmpty(tool.WingetId) && IsInstalledViaWinget(tool.WingetId))
            return true;
        if (!string.IsNullOrEmpty(tool.Name) && IsInstalledViaRegistry(tool.Name))
            return true;
        if (!string.IsNullOrEmpty(tool.LaunchPath) && IsInstalledViaPath(tool.LaunchPath))
            return true;
        return false;
    }

    public static bool IsInstalledViaWinget(string wingetId)
    {
        try
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c winget list --id \"{wingetId}\" --accept-source-agreements 2>nul")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            // winget list outputs the package name if found; if not found, output is minimal
            return output.Contains(wingetId, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsInstalledViaRegistry(string displayName)
    {
        return SearchUninstallKey(RegistryHive.LocalMachine, displayName) ||
               SearchUninstallKey(RegistryHive.CurrentUser, displayName);
    }

    private static bool SearchUninstallKey(RegistryHive hive, string displayName)
    {
        try
        {
            var root = hive == RegistryHive.LocalMachine
                ? Registry.LocalMachine
                : Registry.CurrentUser;

            using var key = root.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key == null) return false;

            foreach (var subName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subName);
                var name = subKey?.GetValue("DisplayName")?.ToString();
                if (name != null && name.Contains(displayName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Also check 32-bit registry on 64-bit systems
            if (hive == RegistryHive.LocalMachine)
            {
                using var key32 = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                if (key32 == null) return false;

                foreach (var subName in key32.GetSubKeyNames())
                {
                    using var subKey = key32.OpenSubKey(subName);
                    var name = subKey?.GetValue("DisplayName")?.ToString();
                    if (name != null && name.Contains(displayName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsInstalledViaPath(string searchPath)
    {
        try
        {
            if (File.Exists(searchPath)) return true;
            if (Directory.Exists(searchPath)) return true;

            // For system tools like devmgmt.msc, check PATH
            if (!Path.IsPathRooted(searchPath) && !searchPath.Contains('\\'))
            {
                var pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
                foreach (var dir in pathVar.Split(';'))
                {
                    var full = Path.Combine(dir.Trim(), searchPath);
                    if (File.Exists(full)) return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
