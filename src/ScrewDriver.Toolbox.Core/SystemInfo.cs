using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace ScrewDriver.Toolbox.Core;

[SupportedOSPlatform("windows")]
public static class SystemInfo
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static Version? WindowsVersion => Environment.OSVersion.Version;

    public static string WindowsVersionString => RuntimeInformation.OSDescription;

    public static bool IsAdministrator
    {
        get
        {
            if (!IsWindows) return false;
            try
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }

    public static string? DetectHardwareBrand()
    {
        if (!IsWindows) return null;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\BIOS");
            return key?.GetValue("SystemManufacturer")?.ToString()?.Trim();
        }
        catch
        {
            return null;
        }
    }

    public static string? DetectSystemModel()
    {
        if (!IsWindows) return null;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\BIOS");
            return key?.GetValue("SystemProductName")?.ToString()?.Trim();
        }
        catch
        {
            return null;
        }
    }

    public static string? GetProcessorName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return key?.GetValue("ProcessorNameString")?.ToString()?.Trim();
        }
        catch
        {
            return null;
        }
    }
}
