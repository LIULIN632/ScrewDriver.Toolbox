using System.Text.Json;
using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class BackupManager
{
    private static readonly string BackupDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriver", "Backup");

    static BackupManager()
    {
        try { if (!Directory.Exists(BackupDir)) Directory.CreateDirectory(BackupDir); }
        catch { }
    }

    public static string CreateFullBackup(string remark, List<SettingSnapshot> snapshots)
    {
        var backup = new BackupPackage
        {
            BackupId = DateTime.Now.ToString("yyyyMMddHHmmss"),
            Remark = remark,
            CreateTime = DateTime.Now,
            ItemCount = snapshots.Count,
            Snapshots = snapshots,
            AppConfig = CaptureAppConfig()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(BackupDir, $"{backup.BackupId}.json"), json);
        Logger.Info($"Backup created: {backup.BackupId} ({snapshots.Count} items)");
        return backup.BackupId;
    }

    public static void RecordSnapshot(string key, string registryPath, string valueName, object? original, object? current)
    {
        var snapshot = new SettingSnapshot
        {
            Key = key,
            RegistryPath = registryPath,
            ValueName = valueName,
            OriginalValue = original,
            CurrentValue = current,
            ModifyTime = DateTime.Now
        };
        var json = JsonSerializer.Serialize(snapshot);
        var dailyFile = Path.Combine(BackupDir, $"changes_{DateTime.Now:yyyyMMdd}.jsonl");
        File.AppendAllText(dailyFile, json + Environment.NewLine);
    }

    public static bool RestoreBackup(string backupId)
    {
        var file = Path.Combine(BackupDir, $"{backupId}.json");
        if (!File.Exists(file)) return false;

        try
        {
            var backup = JsonSerializer.Deserialize<BackupPackage>(File.ReadAllText(file));
            if (backup?.Snapshots == null) return false;

            foreach (var snap in backup.Snapshots)
            {
                if (string.IsNullOrEmpty(snap.RegistryPath)) continue;
                try
                {
                    var parts = snap.RegistryPath.Split('\\', 2);
                    if (parts.Length < 2) continue;

                    var hive = parts[0] switch
                    {
                        "HKEY_LOCAL_MACHINE" or "HKLM" => RegistryHive.LocalMachine,
                        "HKEY_CURRENT_USER" or "HKCU" => RegistryHive.CurrentUser,
                        _ => (RegistryHive?)null
                    };
                    if (hive == null) continue;

                    using var rootKey = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Default);
                    using var subKey = rootKey.OpenSubKey(parts[1], writable: true);
                    subKey?.SetValue(snap.ValueName, snap.OriginalValue ?? string.Empty);
                }
                catch { }
            }

            Logger.Info($"Backup restored: {backupId}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Restore failed: {backupId}", ex);
            return false;
        }
    }

    public static List<BackupPackage> GetBackupList()
    {
        var list = new List<BackupPackage>();
        try
        {
            foreach (var file in Directory.GetFiles(BackupDir, "*.json"))
            {
                try
                {
                    var backup = JsonSerializer.Deserialize<BackupPackage>(File.ReadAllText(file));
                    if (backup != null) list.Add(backup);
                }
                catch { }
            }
        }
        catch { }
        return list.OrderByDescending(b => b.CreateTime).ToList();
    }

    public static bool DeleteBackup(string backupId)
    {
        try
        {
            var file = Path.Combine(BackupDir, $"{backupId}.json");
            if (File.Exists(file)) { File.Delete(file); return true; }
        }
        catch { }
        return false;
    }

    public static string? ExportBackup(string backupId, string targetPath)
    {
        try
        {
            var source = Path.Combine(BackupDir, $"{backupId}.json");
            if (!File.Exists(source)) return null;
            File.Copy(source, targetPath, overwrite: true);
            return targetPath;
        }
        catch { return null; }
    }

    private static Dictionary<string, object> CaptureAppConfig()
    {
        var config = new Dictionary<string, object>();
        try
        {
            var configDir = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var file in Directory.GetFiles(configDir, "*.json"))
            {
                try { config[Path.GetFileName(file)] = File.ReadAllText(file); }
                catch { }
            }
        }
        catch { }
        return config;
    }
}
