using CommunityToolkit.Mvvm.ComponentModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private bool _followSystemTheme = true;

    [ObservableProperty]
    private string _currentVersion = "1.0.0";

    public SettingsViewModel()
    {
        PageTitle = "设置";
        PageDescription = "应用外观、更新与偏好设置";
    }
}
