using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScrewDriver.Toolbox.ViewModels;

/// <summary>
/// ViewModel 基类 - 所有 ViewModel 继承此类
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private string _pageDescription = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    protected virtual void Load()
    {
        // 子类重写
    }
}
