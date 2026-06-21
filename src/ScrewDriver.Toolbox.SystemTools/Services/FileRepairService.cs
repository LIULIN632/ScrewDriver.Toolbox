using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;
using ScrewDriver.Toolbox.SystemTools.Interop;
using static ScrewDriver.Toolbox.SystemTools.Interop.RestartManager;

namespace ScrewDriver.Toolbox.SystemTools.Services;

[SupportedOSPlatform("windows")]
public class FileRepairService
{
    private static readonly string[] CriticalExtensions =
    {
        ".exe", ".lnk", ".msi", ".txt", ".reg", ".bat", ".cmd",
        ".jpg", ".png", ".mp3", ".pdf", ".zip", ".rar", ".docx", ".xlsx"
    };

    private static readonly string[] CommonScanDirs = GetCommonDirs();

    private static string[] GetCommonDirs()
    {
        var dirs = new List<string>();
        try { dirs.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)); } catch { }
        try { dirs.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); } catch { }
        try
        {
            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloads)) dirs.Add(downloads);
        }
        catch { }
        try { dirs.Add(Path.GetTempPath()); } catch { }
        return dirs.Where(Directory.Exists).ToArray();
    }

    // ── File Association ──

    public record AssociationIssue(string Extension, string? ProgId, string? Command, string Issue);

    public List<AssociationIssue> ScanAssociations()
    {
        var issues = new List<AssociationIssue>();

        foreach (var ext in CriticalExtensions)
        {
            try
            {
                var issue = CheckExtension(ext);
                if (issue != null) issues.Add(issue);
            }
            catch { }
        }

        // Image File Execution Options hijack check
        try
        {
            var ifeoKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options");
            if (ifeoKey != null)
            {
                foreach (var ext in CriticalExtensions)
                {
                    var targetExe = GetDefaultExeForExt(ext);
                    if (string.IsNullOrEmpty(targetExe)) continue;
                    var exeName = Path.GetFileName(targetExe);
                    var hijackKey = ifeoKey.OpenSubKey(exeName);
                    if (hijackKey?.GetValue("Debugger") is string debugger && !string.IsNullOrEmpty(debugger))
                    {
                        issues.Add(new AssociationIssue(ext, "IFEO Hijack",
                            debugger, $"映像劫持: {exeName} 被重定向到 {debugger}"));
                    }
                }
            }
        }
        catch { }

        return issues;
    }

    private static AssociationIssue? CheckExtension(string ext)
    {
        using var extKey = Registry.ClassesRoot.OpenSubKey(ext);
        if (extKey == null)
            return new AssociationIssue(ext, null, null, "扩展名未注册");

        var progId = extKey.GetValue(null) as string;
        if (string.IsNullOrEmpty(progId))
            return new AssociationIssue(ext, null, null, "未关联程序");

        // Handle ProgId with on-the-fly override (UserChoice)
        using var userChoice = Registry.CurrentUser.OpenSubKey(
            $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\UserChoice");
        if (userChoice?.GetValue("Progid") is string userProgId)
            progId = userProgId;

        using var cmdKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
        var command = cmdKey?.GetValue(null) as string;

        if (string.IsNullOrEmpty(command))
        {
            // Try to find any shell command
            using var shellKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell");
            if (shellKey == null)
                return new AssociationIssue(ext, progId, null, "无打开命令");

            var subNames = shellKey.GetSubKeyNames();
            foreach (var sub in subNames)
            {
                using var subCmd = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\{sub}\command");
                command = subCmd?.GetValue(null) as string;
                if (!string.IsNullOrEmpty(command)) break;
            }

            if (string.IsNullOrEmpty(command))
                return new AssociationIssue(ext, progId, null, "无打开命令");
        }

        var exePath = ExtractExeFromCommand(command);
        if (string.IsNullOrEmpty(exePath))
            return new AssociationIssue(ext, progId, command, "无法解析程序路径");

        if (!File.Exists(exePath))
            return new AssociationIssue(ext, progId, command, $"程序不存在: {exePath}");

        // Check if the exe is in a suspicious location
        var sysRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        if (!string.IsNullOrEmpty(sysRoot) && exePath.StartsWith(sysRoot, StringComparison.OrdinalIgnoreCase))
            return null; // System directory — trusted
        if (!string.IsNullOrEmpty(programFiles) && exePath.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase))
            return null;
        if (!string.IsNullOrEmpty(programFilesX86) && exePath.StartsWith(programFilesX86, StringComparison.OrdinalIgnoreCase))
            return null;

        // Verify digital signature for non-system paths
        try
        {
            if (File.Exists(exePath))
            {
                var psi = new ProcessStartInfo("powershell.exe",
                    $"-NoProfile -Command \"(Get-AuthenticodeSignature '{exePath}').Status\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = Process.Start(psi);
                if (p != null)
                {
                    p.WaitForExit(5000);
                    var result = p.StandardOutput.ReadToEnd().Trim();
                    if (result == "Valid") return null;
                }
            }
        }
        catch { }

        return new AssociationIssue(ext, progId, command, $"非系统目录程序: {exePath}");
    }

    public void RepairAssociation(string ext, string backupDir)
    {
        Directory.CreateDirectory(backupDir);

        using var extKey = Registry.ClassesRoot.OpenSubKey(ext);
        var progId = extKey?.GetValue(null) as string;
        if (string.IsNullOrEmpty(progId)) return;

        var keyPath = $@"{progId}\shell\open\command";
        using var cmdKey = Registry.ClassesRoot.OpenSubKey(keyPath);
        var currentCmd = cmdKey?.GetValue(null) as string;

        // Backup
        if (!string.IsNullOrEmpty(currentCmd))
        {
            var backupFile = Path.Combine(backupDir, $"{progId}_command.reg");
            var psi = new ProcessStartInfo("reg.exe", $"export \"HKCR\\{keyPath}\" \"{backupFile}\" /y")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi)?.WaitForExit(5000);
        }

        // Restore default via DISM or set safe defaults for known extensions
        var defaults = GetSafeDefaults(ext);
        if (defaults.HasValue)
        {
            using var setKey = Registry.ClassesRoot.CreateSubKey(keyPath);
            setKey?.SetValue(null, defaults.Value);
        }
    }

    private static (string progId, string command)? GetSafeDefaults(string ext)
    {
        return ext.ToLowerInvariant() switch
        {
            ".txt" => ("txtfile", @"%SystemRoot%\system32\NOTEPAD.EXE %1"),
            ".reg" => ("regfile", @"%SystemRoot%\regedit.exe %1"),
            ".bat" => ("batfile", @"""%1"" %*"),
            ".cmd" => ("cmdfile", @"""%1"" %*"),
            ".exe" => ("exefile", @"""%1"" %*"),
            ".lnk" => ("lnkfile", ""), // lnk has no open command — shell handles it
            ".msi" => ("Msi.Package", @"""%SystemRoot%\System32\msiexec.exe"" /i ""%1"""),
            _ => null
        };
    }

    public static string ExtractExeFromCommand(string cmd)
    {
        cmd = Environment.ExpandEnvironmentVariables(cmd).Trim();
        if (string.IsNullOrEmpty(cmd)) return string.Empty;

        // Handle quoted path
        if (cmd.StartsWith('"'))
        {
            var closeQuote = cmd.IndexOf('"', 1);
            if (closeQuote > 1)
                return cmd[1..closeQuote];
        }

        // Unquoted — take up to first space
        var space = cmd.IndexOf(' ');
        return space > 0 ? cmd[..space] : cmd;
    }

    private static string? GetDefaultExeForExt(string ext)
    {
        try
        {
            using var extKey = Registry.ClassesRoot.OpenSubKey(ext);
            var progId = extKey?.GetValue(null) as string;
            if (string.IsNullOrEmpty(progId)) return null;

            using var cmdKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
            var command = cmdKey?.GetValue(null) as string;
            return string.IsNullOrEmpty(command) ? null : ExtractExeFromCommand(command);
        }
        catch { return null; }
    }

    // ── File Lock ──

    public record FileLockInfo(string FilePath, string ProcessName, uint ProcessId, bool Restartable);

    public List<FileLockInfo> ScanFileLocks(int maxFilesPerDir = 80)
    {
        var locked = new List<FileLockInfo>();

        foreach (var dir in CommonScanDirs)
        {
            try
            {
                var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)
                    .Take(maxFilesPerDir)
                    .ToArray();

                if (files.Length == 0) continue;

                var batchResults = CheckFileLocks(files);
                locked.AddRange(batchResults);
            }
            catch { }
        }

        return locked;
    }

    public List<FileLockInfo> CheckFileLocks(string[] filePaths)
    {
        var results = new List<FileLockInfo>();
        if (filePaths.Length == 0) return results;

        uint sessionHandle;
        var startResult = RmStartSession(out sessionHandle, 0, Guid.NewGuid().ToString());
        if (startResult != ERROR_SUCCESS) return results;

        try
        {
            var regResult = RmRegisterResources(sessionHandle, (uint)filePaths.Length, filePaths,
                0, null, 0, null);
            if (regResult != ERROR_SUCCESS) return results;

            uint pnProcInfo = 0;
            uint pnProcInfoNeeded;
            uint rebootReasons = 0;

            var getListResult = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref rebootReasons);
            if (getListResult == ERROR_MORE_DATA && pnProcInfoNeeded > 0)
            {
                var processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;
                var retryResult = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo,
                    processInfo, ref rebootReasons);

                if (retryResult == ERROR_SUCCESS)
                {
                    for (int i = 0; i < pnProcInfo; i++)
                    {
                        var pi = processInfo[i];
                        // Map each process to the files it locks
                        foreach (var file in filePaths)
                        {
                            try
                            {
                                using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
                                // If we get here, the file is NOT locked — skip
                            }
                            catch (IOException)
                            {
                                results.Add(new FileLockInfo(
                                    file, pi.strAppName, pi.Process.dwProcessId, pi.bRestartable));
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            RmEndSession(sessionHandle);
        }

        return results.GroupBy(x => (x.FilePath, x.ProcessId))
            .Select(g => g.First())
            .ToList();
    }

    public static bool TryRmShutdown(string filePath, uint processId)
    {
        try
        {
            var proc = Process.GetProcessById((int)processId);
            if (proc.HasExited) return true;

            uint sessionHandle;
            if (RmStartSession(out sessionHandle, 0, Guid.NewGuid().ToString()) != ERROR_SUCCESS)
                return false;

            try
            {
                if (RmRegisterResources(sessionHandle, 1, new[] { filePath },
                        0, null, 0, null) != ERROR_SUCCESS)
                    return false;

                uint pnProcInfo = 0;
                uint pnProcInfoNeeded;
                uint rebootReasons = 0;

                var listResult = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref rebootReasons);
                if (listResult == ERROR_MORE_DATA && pnProcInfoNeeded > 0)
                {
                    var rg = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;
                    if (RmGetList(sessionHandle, out _, ref pnProcInfo, rg, ref rebootReasons) == ERROR_SUCCESS)
                    {
                        // RmShutdown closes the apps gracefully
                        RmShutdown(sessionHandle, 0, IntPtr.Zero);
                        proc.WaitForExit(10000);
                        return proc.HasExited;
                    }
                }
            }
            finally { RmEndSession(sessionHandle); }
        }
        catch { }
        return false;
    }

    public static bool ForceKillProcess(uint processId)
    {
        try
        {
            var proc = Process.GetProcessById((int)processId);
            if (proc.HasExited) return true;
            proc.Kill();
            proc.WaitForExit(5000);
            return true;
        }
        catch { return false; }
    }

    public static void MarkForDeleteOnReboot(string filePath)
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, uint dwFlags);

        const uint MOVEFILE_DELAY_UNTIL_REBOOT = 0x4;
        MoveFileEx(filePath, null, MOVEFILE_DELAY_UNTIL_REBOOT);
    }
}
