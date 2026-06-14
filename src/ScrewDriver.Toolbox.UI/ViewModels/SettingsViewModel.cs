using System.Reflection;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _version = string.Empty;
    private string _runtime = string.Empty;

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    public string Runtime
    {
        get => _runtime;
        set => SetProperty(ref _runtime, value);
    }

    public SettingsViewModel()
    {
        var asm = Assembly.GetExecutingAssembly();
        Version = asm.GetName().Version?.ToString(3) ?? "1.0.0";
        Runtime = $".NET {Environment.Version.ToString(2)} + WPF";
    }
}
