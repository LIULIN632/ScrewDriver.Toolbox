using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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

        _cleanItems.AddRange(new List<CleanItem>
        {
            new() { Name = "系统临时文件", Description = "Windows Temp 目录中的临时文件", Icon = "📁",
                ScanPaths = { Path.Combine(winDir, "Temp") } },
            new() { Name = "用户临时文件", Description = "用户 %TEMP% 目录中的临时文件", Icon = "📂",
                ScanPaths = { userTemp } },
            new() { Name = "回收站", Description = "已删除但未永久删除的文件", Icon = "🗑️", IsRecycler = true },
            new() { Name = "浏览器缓存", Description = "Chrome/Edge/Firefox 浏览器缓存", Icon = "🌐",
                ScanPaths = {
                    Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache"),
                    Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache"),
                    Path.Combine(localAppData, @"Mozilla\Firefox\Profiles")
                } },
            new() { Name = "缩略图缓存", Description = "Windows 资源管理器缩略图数据库", Icon = "🖼️",
                ScanPaths = { Path.Combine(localAppData, @"Microsoft\Windows\Explorer") } },
            new() { Name = "Windows 更新缓存", Description = "Windows Update 下载缓存", Icon = "🔄",
                ScanPaths = { Path.Combine(winDir, @"SoftwareDistribution\Download") } },
            new() { Name = "日志文件", Description = "系统与应用日志文件", Icon = "📋",
                ScanPaths = { Path.Combine(winDir, "Logs"), Path.Combine(winDir, @"System32\winevt\Logs") } },
            new() { Name = "预读取文件", Description = "Windows Prefetch 预读取数据", Icon = "⚡",
                ScanPaths = { Path.Combine(winDir, "Prefetch") } }
        });

        TxtCategoryCount.Text = _cleanItems.Count.ToString();
    }

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        BtnScan.IsEnabled = false;
        BtnClean.IsEnabled = false;
        TxtResult.Visibility = Visibility.Collapsed;

        await Task.Run(() =>
        {
            foreach (var item in _cleanItems)
            {
                if (item.IsRecycler)
                {
                    var info = GetRecycleBinInfo();
                    item.SizeBytes = info.Size;
                    item.FileCount = info.Count;
                }
                else
                {
                    long totalSize = 0, totalCount = 0;
                    foreach (var path in item.ScanPaths.Where(Directory.Exists))
                        CalcFolderSize(path, ref totalSize, ref totalCount);
                    item.SizeBytes = totalSize;
                    item.FileCount = (int)totalCount;
                }
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
            {
                try { totalSize += new FileInfo(file).Length; totalCount++; }
                catch { }
            }
            foreach (var dir in Directory.GetDirectories(path))
                CalcFolderSize(dir, ref totalSize, ref totalCount);
        }
        catch { }
    }

    private async void BtnClean_Click(object sender, RoutedEventArgs e)
    {
        var selected = _cleanItems.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { WpfMessageBox.Show("请至少选择一个清理项"); return; }

        if (WpfMessageBox.Show("确定要永久删除选中的垃圾文件吗？此操作不可撤销。",
                "确认清理", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.OK)
            return;

        BtnClean.IsEnabled = false;
        BtnScan.IsEnabled = false;

        long freedSize = 0;
        await Task.Run(() =>
        {
            foreach (var item in selected)
            {
                if (item.IsRecycler)
                {
                    EmptyRecycleBin();
                    freedSize += item.SizeBytes;
                    item.SizeBytes = 0;
                    item.FileCount = 0;
                    continue;
                }

                long cleaned = 0;
                int count = 0;
                foreach (var path in item.ScanPaths.Where(Directory.Exists))
                    CleanDir(path, ref cleaned, ref count);

                freedSize += cleaned;
                item.SizeBytes = Math.Max(0, item.SizeBytes - cleaned);
                item.FileCount = Math.Max(0, item.FileCount - count);
            }
        });

        UpdateStatistics();
        TxtResult.Text = $"清理完成！释放了 {FormatBytes(freedSize)} 空间";
        TxtResult.Visibility = Visibility.Visible;
        BtnScan.IsEnabled = true;
        BtnClean.IsEnabled = true;
    }

    private static void CleanDir(string path, ref long freedSize, ref int fileCount)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path))
            {
                try { var fi = new FileInfo(file); freedSize += fi.Length; fi.Delete(); fileCount++; }
                catch { }
            }
            foreach (var dir in Directory.GetDirectories(path))
            {
                CleanDir(dir, ref freedSize, ref fileCount);
                try { if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0) Directory.Delete(dir); }
                catch { }
            }
        }
        catch { }
    }

    // Win32 API for Recycle Bin
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? pszRootPath, out SHQUERYRBINFO pSHQueryRBInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

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

    private void UpdateStatistics()
    {
        TxtTotalSize.Text = FormatBytes(_cleanItems.Sum(x => x.SizeBytes));
        TxtTotalCount.Text = _cleanItems.Sum(x => x.FileCount).ToString();
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int i = 0;
        while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
        return $"{size:0.##} {units[i]}";
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
