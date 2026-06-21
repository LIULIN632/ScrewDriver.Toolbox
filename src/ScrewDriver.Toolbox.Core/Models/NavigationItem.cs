using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ScrewDriver.Toolbox.Core.Models;

public class NavigationItem : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isActive;

    public string Title { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;

    public bool IsExpanded
    {
        get => _isExpanded;
        set { if (_isExpanded != value) { _isExpanded = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded))); } }
    }

    public bool IsActive
    {
        get => _isActive;
        set { if (_isActive != value) { _isActive = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive))); } }
    }

    public ObservableCollection<NavigationItem> SubItems { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
}
