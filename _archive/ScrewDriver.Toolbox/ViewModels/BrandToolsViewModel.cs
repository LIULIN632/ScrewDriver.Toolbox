using CommunityToolkit.Mvvm.ComponentModel;
using ScrewDriver.Toolbox.Models;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class BrandToolsViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _detectedBrand = string.Empty;

    [ObservableProperty]
    private string _detectedModel = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ToolModel> _recommendedTools = [];

    public BrandToolsViewModel()
    {
        PageTitle = "品牌工具";
        PageDescription = "自动识别设备品牌，推荐专属工具（开发中）";
    }
}
