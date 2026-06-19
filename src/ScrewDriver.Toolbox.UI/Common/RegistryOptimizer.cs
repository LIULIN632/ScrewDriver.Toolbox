using Microsoft.Win32;

namespace ScrewDriver.Toolbox.UI.Common;

/// <summary>通用的注册表优化设置读写</summary>
public static class RegistryOptimizer
{
    public static bool IsWindows10OrLater() => Environment.OSVersion.Version.Major >= 10;
    public static bool IsWindows11OrLater() => Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;
    public static bool FileExists(string path) => System.IO.File.Exists(path);

    public static bool ReadBool(string keyPath, string valueName, int expectedValue = 1)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(keyPath) 
                         ?? Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) return false;
            var val = key.GetValue(valueName);
            return val is int i && i == expectedValue;
        }
        catch { return false; }
    }

    public static int ReadDword(string subKey, string valueName, int defaultValue = 0)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(subKey);
            if (key == null) return defaultValue;
            var val = key.GetValue(valueName);
            return val is int i ? i : defaultValue;
        }
        catch { return defaultValue; }
    }

    public static bool WriteDword(string subKey, string valueName, int value)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(subKey, true);
            if (key == null) return false;
            key.SetValue(valueName, value, RegistryValueKind.DWord);
            return true;
        }
        catch { return false; }
    }

    public static bool WriteBool(string keyPath, string valueName, bool enable, int trueVal = 1, int falseVal = 0)
    {
        try
        {
            var hive = keyPath.StartsWith("HKEY_LOCAL_MACHINE") ? Registry.LocalMachine : Registry.CurrentUser;
            var subKey = keyPath.Contains("\\") 
                ? keyPath[(keyPath.IndexOf('\\', keyPath.IndexOf('\\') + 1) + 1)..] 
                : keyPath;
            
            // Handle both HKEY_CURRENT_USER\... and HKEY_LOCAL_MACHINE\... formats
            if (keyPath.StartsWith("HKEY_CURRENT_USER\\"))
                subKey = keyPath["HKEY_CURRENT_USER\\".Length..];
            else if (keyPath.StartsWith("HKEY_LOCAL_MACHINE\\"))
                subKey = keyPath["HKEY_LOCAL_MACHINE\\".Length..];
            
            using var key = hive.CreateSubKey(subKey, true);
            if (key == null) return false;
            key.SetValue(valueName, enable ? trueVal : falseVal, RegistryValueKind.DWord);
            return true;
        }
        catch { return false; }
    }
}
