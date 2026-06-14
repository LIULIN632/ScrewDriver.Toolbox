using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScrewDriver.Toolbox.Models;
using ScrewDriver.Toolbox.Services;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class ToolRepositoryViewModel : BaseViewModel
{
    private readonly IToolCatalogService _catalogService;

    [ObservableProperty]
    private ObservableCollection<string> _categories = [];

    [ObservableProperty]
    private string _selectedCategory = "全部";

    [ObservableProperty]
    private ObservableCollection<ToolModel> _tools = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ToolModel? _selectedTool;

    public ToolRepositoryViewModel(IToolCatalogService catalogService)
    {
        _catalogService = catalogService;
        PageTitle = "工具仓库";
        PageDescription = "发现并使用精选 Windows 工具";

        Categories = catalogService.GetCategories();
        Categories.Insert(0, "全部");
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (value == "全部")
            Tools = _catalogService.SearchTools(SearchQuery);
        else
            Tools = _catalogService.GetToolsByCategory(value);
    }

    partial void OnSearchQueryChanged(string value)
    {
        Tools = _catalogService.SearchTools(value);
    }

    [RelayCommand]
    private void LoadTools()
    {
        Tools = _catalogService.SearchTools(SearchQuery);
    }

    [RelayCommand]
    private void SelectTool(ToolModel tool)
    {
        SelectedTool = tool;
    }

    [RelayCommand]
    private void InstallTool(ToolModel tool)
    {
        // TODO: 启动安装流程
    }

    [RelayCommand]
    private void OpenToolUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
    }
}
