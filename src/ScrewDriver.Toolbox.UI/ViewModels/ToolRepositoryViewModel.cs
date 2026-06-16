using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ScrewDriver.Toolbox.Core.Models;
using System.IO;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Views;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class ToolRepositoryViewModel : BaseViewModel
{
    private readonly ToolLauncher _launcher = new();
    private readonly RecentToolsService _recentService = new();
    private string _searchText = string.Empty;
    private string _selectedCategory = "全部";
    private bool _isScanning;
    private bool _scanStarted;
    private bool _hasChecked;
    private bool _isCheckingForUpdates;

    public ObservableCollection<ToolItem> AllTools { get; }
    public ObservableCollection<ToolItem> FilteredTools { get; } = new();
    public ObservableCollection<ToolItem> RecommendedTools { get; } = new();

    public bool IsScanning
    {
        get => _isScanning;
        set => SetProperty(ref _isScanning, value);
    }

    public bool IsCheckingForUpdates
    {
        get => _isCheckingForUpdates;
        set => SetProperty(ref _isCheckingForUpdates, value);
    }

    public List<string> Categories { get; } = new()
    {
        "全部", "系统工具", "CPU工具", "主板工具", "内存工具",
        "显卡工具", "硬盘工具", "屏幕工具", "外设工具",
        "安全工具", "品牌工具", "启动与镜像",
        "游戏工具", "烤鸡工具", "综合检测", "其他工具"
    };

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) FilterTools(); }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { if (SetProperty(ref _selectedCategory, value)) FilterTools(); }
    }

    public RelayCommand LaunchToolCommand { get; }
    public RelayCommand InstallToolCommand { get; }
    public RelayCommand OpenUrlCommand { get; }
    public RelayCommand ScanLocalFolderCommand { get; }
    public RelayCommand ClearLocalScanCommand { get; }
    public RelayCommand CheckForUpdatesCommand { get; }
    public RelayCommand UpgradeToolCommand { get; }

    public ToolRepositoryViewModel()
    {
        AllTools = new ObservableCollection<ToolItem>(ToolRegistry.GetAllTools());
        FilterTools();
        LoadRecommendedTools();

        LaunchToolCommand = new RelayCommand(param =>
        {
            if (param is not ToolItem tool || string.IsNullOrEmpty(tool.LaunchPath))
                return;

            if (tool.LaunchPath.StartsWith("scenario:"))
            {
                var scenarioName = tool.LaunchPath["scenario:".Length..];
                if (scenarioName == "game-accelerate" &&
                    Application.Current.MainWindow is MainWindow mw)
                {
                    mw.NavigateToPageByTag("SystemOptimizerPage");
                }
                return;
            }

            if (tool.LaunchPath.StartsWith("ms-") || tool.LaunchPath.StartsWith("http"))
            {
                try { Process.Start(new ProcessStartInfo(tool.LaunchPath) { UseShellExecute = true }); }
                catch (Exception ex)
                {
                    MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            _launcher.Launch(tool.LaunchPath);
            _recentService.AddTool(tool.Name, "ToolRepositoryPage");
        });

        InstallToolCommand = new RelayCommand(param =>
        {
            if (param is not ToolItem tool) return;

            if (!string.IsNullOrEmpty(tool.WingetId))
            {
                var result = MessageBox.Show(
                    $"即将使用 winget 安装「{tool.Name}」。\n\n确定继续？",
                    "Winget 安装", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/k winget install {tool.WingetId} --accept-package-agreements")
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"安装启动失败: {ex.Message}",
                        "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (!string.IsNullOrEmpty(tool.OfficialUrl))
            {
                var result = MessageBox.Show(
                    $"该工具无法自动安装，将打开官方下载页面。\n\n{tool.OfficialUrl}\n\n是否继续？",
                    "手动下载", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    Process.Start(new ProcessStartInfo(tool.OfficialUrl) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法打开链接: {ex.Message}",
                        "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("暂无自动安装源，请手动下载。", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });

        OpenUrlCommand = new RelayCommand(param =>
        {
            if (param is not string url || string.IsNullOrWhiteSpace(url)) return;

            var result = MessageBox.Show(
                $"即将访问: {url}\n\n是否继续？",
                "打开链接", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes) return;

            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        });

        ScanLocalFolderCommand = new RelayCommand(_ => ScanLocalFolder());
        ClearLocalScanCommand = new RelayCommand(_ => ClearLocalScan());

        CheckForUpdatesCommand = new RelayCommand(_ => CheckForUpdatesAsync());

        UpgradeToolCommand = new RelayCommand(param =>
        {
            if (param is ToolItem tool && !string.IsNullOrEmpty(tool.WingetId))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("cmd.exe",
                        $"/c start \"\" winget upgrade {tool.WingetId}")
                    { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"启动更新失败: {ex.Message}",
                        "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        });

        // Deferred: installation scan will be triggered by StartDetection()
        // Deferred: auto update check after 3 seconds
        _ = Task.Delay(3000).ContinueWith(_ =>
        {
            if (!_hasChecked)
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => CheckForUpdatesAsync()), DispatcherPriority.Background);
        });
    }

    public async void StartDetection()
    {
        if (_scanStarted) return;
        _scanStarted = true;
        await ScanInstallationsAsync();
    }

    private async Task ScanInstallationsAsync()
    {
        IsScanning = true;
        var tools = AllTools.ToList();

        await Task.Run(() =>
        {
            foreach (var tool in tools)
            {
                var installed = InstallationDetector.IsToolInstalled(tool);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    tool.IsInstalled = installed;
                    OnPropertyChanged(nameof(tool.IsInstalled));
                }, DispatcherPriority.Background);

                // Brief pause between checks to avoid thrashing
                if (tool.WingetId?.Length > 0)
                    Thread.Sleep(120); // winget queries are slow
            }
        });

        IsScanning = false;
    }

    private void ScanLocalFolder()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "选择包含 .exe 工具的文件夹",
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            return;

        var folder = dialog.SelectedPath;
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            return;

        var exeFiles = Directory.GetFiles(folder, "*.exe", SearchOption.TopDirectoryOnly);
        if (exeFiles.Length == 0)
        {
            System.Windows.MessageBox.Show("所选文件夹中未找到 .exe 文件。", "提示",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        var iconCacheDir = Path.Combine(Path.GetTempPath(), "ScrewDriver.Toolbox", "icons");
        var matchCount = 0;

        foreach (var exePath in exeFiles)
        {
            var exeName = Path.GetFileNameWithoutExtension(exePath);

            foreach (var tool in AllTools)
            {
                if (tool.IsInstalled && tool.LocalExePath != null)
                    continue; // already matched via local scan

                if (FuzzyMatchExe(exeName, tool))
                {
                    tool.IsInstalled = true;
                    tool.LocalExePath = exePath;

                    var iconPath = IconExtractor.ExtractAndSaveIcon(exePath, iconCacheDir);
                    if (iconPath != null)
                        tool.IconPath = iconPath;

                    matchCount++;
                    break;
                }
            }
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            // Refresh filtered list
            FilterTools();
        });

        System.Windows.MessageBox.Show(
            $"扫描完成：在 {exeFiles.Length} 个 exe 中匹配到 {matchCount} 个已知工具。",
            "本地扫描完成", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

        _ = InstalledToolsCache.Instance.RefreshAsync();
    }

    private void ClearLocalScan()
    {
        foreach (var tool in AllTools)
        {
            tool.LocalExePath = null;
            tool.IconPath = null;
            // Don't clear IsInstalled — winget detection result is preserved
        }
        FilterTools();
        _ = InstalledToolsCache.Instance.RefreshAsync();
    }

    private static bool FuzzyMatchExe(string exeName, ToolItem tool)
    {
        // Direct substring match (case-insensitive)
        if (exeName.Contains(tool.Name, StringComparison.OrdinalIgnoreCase))
            return true;
        if (tool.Name.Contains(exeName, StringComparison.OrdinalIgnoreCase))
            return true;

        // Match against WingetId (e.g. "CPUID.CPU-Z" → "CPU-Z")
        if (!string.IsNullOrEmpty(tool.WingetId))
        {
            var wingetShort = tool.WingetId.Split('.').Last();
            if (exeName.Contains(wingetShort, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Match against LaunchPath (e.g. "devmgmt.msc" → no match with exe)
        if (!string.IsNullOrEmpty(tool.LaunchPath))
        {
            var launchName = Path.GetFileNameWithoutExtension(tool.LaunchPath);
            if (!string.IsNullOrEmpty(launchName) &&
                exeName.Contains(launchName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public void AddDroppedTool(string exePath)
    {
        // Check if already exists
        var exeName = Path.GetFileNameWithoutExtension(exePath);
        var existing = AllTools.FirstOrDefault(t =>
            t.LocalExePath != null && t.LocalExePath.Equals(exePath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.IsInstalled = true;
            existing.LocalExePath = exePath;
            var existIcon = IconExtractor.ExtractAndSaveIcon(exePath,
                Path.Combine(Path.GetTempPath(), "ScrewDriver.Toolbox", "icons"));
            if (existIcon != null) existing.IconPath = existIcon;
            FilterTools();
            return;
        }

        // Try to match with known tool
        var matched = AllTools.FirstOrDefault(t => FuzzyMatchExe(exeName, t));
        if (matched != null)
        {
            matched.IsInstalled = true;
            matched.LocalExePath = exePath;
            var matchIcon = IconExtractor.ExtractAndSaveIcon(exePath,
                Path.Combine(Path.GetTempPath(), "ScrewDriver.Toolbox", "icons"));
            if (matchIcon != null) matched.IconPath = matchIcon;
            FilterTools();
            return;
        }

        // Show dialog for new custom tool
        var dialog = new AddToolDialog(exePath, ToolRegistry.Categories.ToList());
        dialog.Owner = Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            var newTool = new ToolItem
            {
                Name = dialog.ToolName,
                LaunchPath = dialog.ToolPath,
                LocalExePath = dialog.ToolPath,
                Category = dialog.SelectedCategory,
                Description = $"用户添加: {Path.GetFileName(exePath)}",
                IsInstalled = true,
                IsCustom = true
            };

            var newIcon = IconExtractor.ExtractAndSaveIcon(exePath,
                Path.Combine(Path.GetTempPath(), "ScrewDriver.Toolbox", "icons"));
            if (newIcon != null) newTool.IconPath = newIcon;

            ToolRegistry.AddCustomTool(newTool);
            AllTools.Add(newTool);
            FilterTools();
            _ = InstalledToolsCache.Instance.RefreshAsync();
        }
    }

    private void FilterTools()
    {
        FilteredTools.Clear();
        var query = AllTools.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(t =>
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (SelectedCategory != "全部")
            query = query.Where(t => t.Category == SelectedCategory);

        foreach (var tool in query)
            FilteredTools.Add(tool);
    }

    private async void CheckForUpdatesAsync()
    {
        if (_isCheckingForUpdates) return;
        _hasChecked = true;
        IsCheckingForUpdates = true;

        await Task.Run(() =>
        {
            var updatableIds = UpdateChecker.GetUpdatableWingetIds();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var tool in AllTools)
                {
                    tool.HasUpdate = !string.IsNullOrEmpty(tool.WingetId) &&
                                     updatableIds.Contains(tool.WingetId);
                }
                FilterTools();
            });
        });

        IsCheckingForUpdates = false;
    }

    private void LoadRecommendedTools()
    {
        var recommendedNames = new[] { "Everything", "Geek Uninstaller", "CPU-Z", "Dism++", "磁盘分析", "硬件监控", "网速测试", "磁盘测试" };
        foreach (var name in recommendedNames)
        {
            var tool = AllTools.FirstOrDefault(t =>
                t.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                name.Contains(t.Name, StringComparison.OrdinalIgnoreCase));
            if (tool != null && !RecommendedTools.Contains(tool))
                RecommendedTools.Add(tool);
        }
    }

}