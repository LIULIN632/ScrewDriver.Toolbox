using System.Reflection;
using ScrewDriver.Toolbox.Core.Services;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _version = string.Empty;
    private string _runtime = string.Empty;
    private string _toolCount = string.Empty;

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

    public string ToolCount
    {
        get => _toolCount;
        set => SetProperty(ref _toolCount, value);
    }

    public SettingsViewModel()
    {
        var asm = Assembly.GetExecutingAssembly();
        Version = asm.GetName().Version?.ToString(3) ?? "1.0.0";
        Runtime = $".NET {Environment.Version.ToString(2)} + WPF";

        var count = ToolRegistry.GetAllTools().Count;
        var cats = ToolRegistry.Categories.Count;
        ToolCount = $"{count} 个工具 · {cats} 个分类";
    }
}
