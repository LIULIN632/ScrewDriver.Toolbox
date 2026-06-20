using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class StartPageViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private string _selectedCategory = "全部";
    private string _windowsVersion = string.Empty;
    private string _isAdmin = string.Empty;
    private string _deviceBrand = string.Empty;

    public ObservableCollection<ToolItem> FilteredTools { get; } = new();

    public List<string> Categories { get; } = new() { "全部" };

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) _toolsView.Refresh(); }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { if (SetProperty(ref _selectedCategory, value)) _toolsView.Refresh(); }
    }

    public string WindowsVersion
    {
        get => _windowsVersion;
        set => SetProperty(ref _windowsVersion, value);
    }

    public string IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    public string DeviceBrand
    {
        get => _deviceBrand;
        set => SetProperty(ref _deviceBrand, value);
    }

    public ICommand LaunchToolCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand TogglePinCommand { get; }
    public ICommand RemoveToolCommand { get; }

    private readonly ICollectionView _toolsView;
    private readonly HashSet<string> _hiddenTools = new();
    private static readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);

    public StartPageViewModel()
    {
        var saved = _config.Load<HiddenToolsModel>("hidden-tools");
        if (saved?.Names != null)
        {
            foreach (var name in saved.Names)
                _hiddenTools.Add(name);
        }

        // Build category list from registry
        Categories.AddRange(ToolRegistry.Categories);

        WindowsVersion = SystemInfo.WindowsVersionString ?? "Windows 10/11";
        IsAdmin = SystemInfo.IsAdministrator ? "管理员" : "标准用户";
        DeviceBrand = SystemInfo.DetectHardwareBrand() ?? "未知设备";

        LaunchToolCommand = new RelayCommand(LaunchTool);
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);
        TogglePinCommand = new RelayCommand(param =>
        {
            if (param is not string name) return;
            InstalledToolsCache.Instance.TogglePin(name);
        });

        RemoveToolCommand = new RelayCommand(param =>
        {
            if (param is string name)
            {
                _hiddenTools.Add(name);
                _config.Save("hidden-tools", new HiddenToolsModel { Names = _hiddenTools.ToList() });
                RefreshFromCache();
            }
        });

        _toolsView = CollectionViewSource.GetDefaultView(FilteredTools);
        _toolsView.Filter = FilterToolItem;
        _toolsView.SortDescriptions.Add(new SortDescription("IsPinned", ListSortDirection.Descending));
        _toolsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        WeakEventManager<InstalledToolsCache, EventArgs>.AddHandler(
            InstalledToolsCache.Instance, nameof(InstalledToolsCache.CacheUpdated), OnCacheUpdated);
        RefreshFromCache();
    }

    private void OnCacheUpdated(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(RefreshFromCache);
    }

    private void RefreshFromCache()
    {
        FilteredTools.Clear();
        var all = new List<ToolItem>();
        all.AddRange(InstalledToolsCache.Instance.InstalledTools);
        all.AddRange(InstalledToolsCache.Instance.ToolsFolderTools);

        foreach (var t in all.Where(t => !_hiddenTools.Contains(t.Name)))
            FilteredTools.Add(t);
    }

    private bool FilterToolItem(object obj)
    {
        if (obj is not ToolItem tool) return false;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            if (!tool.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                !tool.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (SelectedCategory != "全部" && tool.Category != SelectedCategory)
            return false;

        return true;
    }

    private static void LaunchTool(object? parameter)
    {
        if (parameter is not ToolItem tool) return;

        string? path = tool.LocalExePath ?? tool.LaunchPath;
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show($"工具文件缺失\n\n{tool.Name} 的程序文件未找到。",
                "启动失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}

internal class HiddenToolsModel { public List<string> Names { get; set; } = new(); }
