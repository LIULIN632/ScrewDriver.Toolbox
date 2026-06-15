using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using WpfApp = System.Windows.Application;
using WpfMessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class CleanWindow : Window
{
    private readonly List<CleanItem> _cleanItems = new();

    public CleanWindow()
    {
        InitializeComponent();
        InitCleanItems();
        ItemsClean.ItemsSource = _cleanItems;
        UpdateStatistics();
    }

    private void InitCleanItems()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var userTemp = Path.GetTempPath();
        var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var systemDrive = Path.GetPathRoot(winDir) ?? "C:\\";

        _cleanItems.AddRange(new List<CleanItem>
        {
            // ===== 基础清理（默认勾选） =====
            new() { Name = "系统临时文件", Description = "Windows Temp 目录中的临时文件", Icon = "📁", IsSelected = true,
                ScanPaths = { Path.Combine(winDir, "Temp") } },
            new() { Name = "用户临时文件", Description = "用户 %TEMP% 目录中的临时文件", Icon = "📂", IsSelected = true,
                ScanPaths = { userTemp } },
            new() { Name = "回收站", Description = "已删除但未永久删除的文件", Icon = "🗑️", IsSelected = true, IsRecycler = true },
            new() { Name = "浏览器缓存", Description = "Chrome/Edge/Firefox 浏览器缓存", Icon = "🌐", IsSelected = true,
                ScanPaths = {
                    Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache"),
                    Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache"),
                    Path.Combine(localAppData, @"Mozilla\Firefox\Profiles")
                } },
            new() { Name = "缩略图缓存", Description = "Windows 资源管理器缩略图数据库", Icon = "🖼️", IsSelected = true,
                ScanPaths = { Path.Combine(localAppData, @"Microsoft\Windows\Explorer") } },
            new() { Name = "Windows 更新缓存", Description = "Windows Update 下载缓存", Icon = "🔄", IsSelected = true,
                ScanPaths = { Path.Combine(winDir, @"SoftwareDistribution\Download") } },
            new() { Name = "日志文件", Description = "系统与应用日志文件", Icon = "📋", IsSelected = true,
                ScanPaths = { Path.Combine(winDir, "Logs"), Path.Combine(winDir, @"System32\winevt\Logs") } },
            new() { Name = "预读取文件", Description = "Windows Prefetch 预读取数据", Icon = "⚡", IsSelected = true,
                ScanPaths = { Path.Combine(winDir, "Prefetch") } },

            // ===== 深度清理（默认勾选） =====
            new() { Name = "系统崩溃转储", Description = "蓝屏、程序崩溃生成的调试日志文件", Icon = "💥", IsSelected = true,
                ScanPaths = { Path.Combine(winDir, "Minidump"), Path.Combine(winDir, "MEMORY.DMP") } },
            new() { Name = "传递优化缓存", Description = "Windows 更新 P2P 分发缓存", Icon = "📡", IsSelected = true,
                ScanPaths = { Path.Combine(winDir, @"SoftwareDistribution\DeliveryOptimization") } },
            new() { Name = "系统错误报告", Description = "应用与系统崩溃的诊断报告数据", Icon = "📄", IsSelected = true,
                ScanPaths = { Path.Combine(commonAppData, @"Microsoft\Windows\WER") } },
            new() { Name = "微软商店缓存", Description = "应用商店下载残留与临时缓存", Icon = "🏪", IsSelected = true,
                ScanPaths = { Path.Combine(localAppData, @"Packages\Microsoft.WindowsStore_8wekyb3d8bbwe\LocalCache") } },

            // ===== 高风险项（默认不勾选） =====
            new() { Name = "Windows.old 旧系统", Description = "系统升级残留，清理后无法回退旧版本", Icon = "⚠️", IsSelected = false, IsRisk = true,
                ScanPaths = { Path.Combine(systemDrive, "Windows.old") } },
            new() { Name = "系统组件存储(WinSxS)", Description = "调用 DISM 清理过期组件，安全可靠", Icon = "🔧", IsSelected = false, IsRisk = true,
                SpecialType = "WinSxS" },
        });

        TxtCategoryCount.Text = _cleanItems.Count.ToString();
    }

    // ==================== 扫描 ====================

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        BtnScan.IsEnabled = false;
        BtnClean.IsEnabled = false;
        TxtResult.Visibility = Visibility.Collapsed;

        await Task.Run(() =>
        {
            foreach (var item in _cleanItems)
            {
                if (item.SpecialType == "WinSxS") { item.SizeBytes = 0; item.FileCount = 0; continue; }
                if (item.IsRecycler) { var info = GetRecycleBinInfo(); item.SizeBytes = info.Size; item.FileCount = info.Count; continue; }

                long totalSize = 0, totalCount = 0;
                foreach (var path in item.ScanPaths)
                {
                    try
                    {
                        if (Directory.Exists(path)) CalcFolderSize(path, ref totalSize, ref totalCount);
                        else if (File.Exists(path)) { totalSize += new FileInfo(path).Length; totalCount++; }
                    }
                    catch { }
                }
                item.SizeBytes = totalSize;
                item.FileCount = (int)totalCount;
            }
        });

        UpdateStatistics();
        BtnScan.IsEnabled = true;
        BtnClean.IsEnabled = true;
    }

    private static void CalcFolderSize(string path, ref long totalSize, ref long totalCount)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path))
                try { totalSize += new FileInfo(file).Length; totalCount++; } catch { }
            foreach (var dir in Directory.GetDirectories(path))
                CalcFolderSize(dir, ref totalSize, ref totalCount);
        }
        catch { }
    }

    // ==================== 清理 ====================

    private async void BtnClean_Click(object sender, RoutedEventArgs e)
    {
        var selected = _cleanItems.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { WpfMessageBox.Show("请至少选择一个清理项"); return; }

        // 高风险项二次确认
        var riskItems = selected.Where(x => x.IsRisk).ToList();
        if (riskItems.Any())
        {
            var names = string.Join("、", riskItems.Select(x => x.Name));
            if (WpfMessageBox.Show($"您选中了高风险项：{names}\n\n此操作不可撤销，确认继续？",
                    "风险确认", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;
        }
        else
        {
            if (WpfMessageBox.Show("确定要永久删除选中的垃圾文件吗？此操作不可撤销。",
                    "确认清理", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                return;
        }

        BtnClean.IsEnabled = false;
        BtnScan.IsEnabled = false;

        long freedSize = 0;
        await Task.Run(() =>
        {
            foreach (var item in selected)
            {
                if (item.SpecialType == "WinSxS") { CleanWinSxS(); continue; }
                if (item.IsRecycler) { EmptyRecycleBin(); freedSize += item.SizeBytes; item.SizeBytes = 0; item.FileCount = 0; continue; }

                long cleaned = 0, count = 0;
                foreach (var path in item.ScanPaths)
                {
                    try
                    {
                        if (Directory.Exists(path)) CleanDir(path, ref cleaned, ref count);
                        else if (File.Exists(path)) { var fi = new FileInfo(path); cleaned += fi.Length; fi.Delete(); count++; }
                    }
                    catch { }
                }
                freedSize += cleaned;
                item.SizeBytes = Math.Max(0, item.SizeBytes - cleaned);
                item.FileCount = Math.Max(0, item.FileCount - (int)count);
            }
        });

        UpdateStatistics();
        TxtResult.Text = $"清理完成！释放了 {FormatBytes(freedSize)} 空间";
        TxtResult.Visibility = Visibility.Visible;
        BtnScan.IsEnabled = true;
        BtnClean.IsEnabled = true;
    }

    private static void CleanDir(string path, ref long freedSize, ref long fileCount)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path))
                try { var fi = new FileInfo(file); freedSize += fi.Length; fi.Delete(); fileCount++; } catch { }
            foreach (var dir in Directory.GetDirectories(path))
            {
                CleanDir(dir, ref freedSize, ref fileCount);
                try { if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0) Directory.Delete(dir); } catch { }
            }
        }
        catch { }
    }

    // ==================== WinSxS + 休眠 ====================

    private static void CleanWinSxS()
    {
        try
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dism.exe",
                    Arguments = "/online /Cleanup-Image /StartComponentCleanup",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            p.Start();
            p.WaitForExit();
        }
        catch { }
    }

    private void BtnDisableHibernate_Click(object sender, RoutedEventArgs e)
    {
        if (WpfMessageBox.Show("关闭休眠将删除 hiberfil.sys 文件，\n可释放等同内存容量的 C 盘空间。\n\n确定继续？",
                "关闭休眠", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK)
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = "-h off",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            });
            WpfMessageBox.Show("休眠已关闭，hiberfil.sys 已自动删除。", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch
        {
            WpfMessageBox.Show("关闭休眠失败，请以管理员身份运行。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ==================== 回收站 Win32 API ====================

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? pszRootPath, out SHQUERYRBINFO pSHQueryRBInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct SHQUERYRBINFO { public int cbSize; public long i64Size; public long i64NumItems; }

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    private static (long Size, int Count) GetRecycleBinInfo()
    {
        try
        {
            var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf<SHQUERYRBINFO>() };
            SHQueryRecycleBin(null, out info);
            return (info.i64Size, (int)info.i64NumItems);
        }
        catch { return (0, 0); }
    }

    private static void EmptyRecycleBin()
    {
        try
        {
            WpfApp.Current.Dispatcher.Invoke(() =>
                SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND));
        }
        catch { }
    }

    // ==================== 辅助 ====================

    private void UpdateStatistics()
    {
        TxtTotalSize.Text = FormatBytes(_cleanItems.Sum(x => x.SizeBytes));
        TxtTotalCount.Text = _cleanItems.Sum(x => x.FileCount).ToString();
    }

    private static string FormatBytes(long bytes)
    {
        string[] u = { "B", "KB", "MB", "GB", "TB" };
        double s = bytes; int i = 0;
        while (s >= 1024 && i < u.Length - 1) { s /= 1024; i++; }
        return $"{s:0.##} {u[i]}";
    }

    private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _cleanItems) item.IsSelected = true;
    }

    private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _cleanItems) item.IsSelected = false;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
