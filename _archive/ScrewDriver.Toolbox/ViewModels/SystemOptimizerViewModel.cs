using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScrewDriver.Toolbox.Models;
using ScrewDriver.Toolbox.Services;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class SystemOptimizerViewModel : BaseViewModel
{
    private readonly ISystemOptimizerService _optimizerService;

    [ObservableProperty]
    private ObservableCollection<string> _categories = [];

    [ObservableProperty]
    private string _selectedCategory = "基础设置";

    [ObservableProperty]
    private ObservableCollection<SettingItem> _settings = [];

    public SystemOptimizerViewModel(ISystemOptimizerService optimizerService)
    {
        _optimizerService = optimizerService;
        PageTitle = "系统优化";
        PageDescription = "调整 Windows 设置，提升系统性能与体验";

        Categories = optimizerService.GetCategories();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        Settings = _optimizerService.GetSettingsByCategory(value);
    }

    [RelayCommand]
    private void LoadSettings()
    {
        Settings = _optimizerService.GetSettingsByCategory(SelectedCategory);
    }

    [RelayCommand]
    private void ToggleSetting(SettingItem item)
    {
        _optimizerService.ApplySetting(item, !item.IsEnabled);
    }

    [RelayCommand]
    private void RestoreSetting(SettingItem item)
    {
        _optimizerService.RestoreSetting(item.Id);
    }

    [RelayCommand]
    private void CreateRestorePoint()
    {
        _optimizerService.CreateRestorePoint();
    }
}
