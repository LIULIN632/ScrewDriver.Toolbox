using System.IO;

namespace ScrewDriver.Toolbox.Core.Services;

public static class Logger
{
    private static readonly string LogDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScrewDriver", "Logs");
    private static readonly object LockObj = new();

    static Logger()
    {
        try { if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir); }
        catch { }
        CleanOldLogs();
    }

    public static void Info(string message) => WriteLog("INFO", message);
    public static void Warn(string message) => WriteLog("WARN", message);
    public static void Error(string message, Exception? ex = null)
        => WriteLog("ERROR", $"{message} {ex?.Message} {ex?.StackTrace}");

    public static string[] GetLogFiles()
    {
        try { return Directory.GetFiles(LogDir, "*.log").OrderByDescending(f => f).ToArray(); }
        catch { return Array.Empty<string>(); }
    }

    public static string ReadLog(string path)
    {
        try { return File.ReadAllText(path); }
        catch { return string.Empty; }
    }

    private static void WriteLog(string level, string message)
    {
        lock (LockObj)
        {
            try
            {
                string logFile = Path.Combine(LogDir, $"{DateTime.Now:yyyyMMdd}.log");
                string line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
            catch { }
        }
    }

    private static void CleanOldLogs()
    {
        try
        {
            foreach (var file in Directory.GetFiles(LogDir, "*.log"))
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-7))
                    File.Delete(file);
            }
        }
        catch { }
    }
}
