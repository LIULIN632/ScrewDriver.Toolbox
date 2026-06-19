using System.Collections.ObjectModel;
using ScrewDriver.Toolbox.UI.ViewModels;

namespace ScrewDriver.Toolbox.UI.Common;

public class RegistryOption
{
    public string DisplayName { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>支持多选项的注册表设置项</summary>
public class RegistryComboItem : BaseViewModel
{
    private int _selectedValue;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public string SubKey { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public ObservableCollection<RegistryOption> Options { get; } = new();

    public int SelectedValue
    {
        get => _selectedValue;
        set
        {
            _selectedValue = value;
            OnPropertyChanged();
            Save();
        }
    }

    public RegistryComboItem()
    {
        // Load current value from registry
        _selectedValue = RegistryOptimizer.ReadDword(SubKey, ValueName);
    }

    private void Save()
    {
        RegistryOptimizer.WriteDword(SubKey, ValueName, SelectedValue);
        ShellChangeNotifier.Notify(ShellChangeNotifier.RefreshScope.Taskbar);
    }
}
