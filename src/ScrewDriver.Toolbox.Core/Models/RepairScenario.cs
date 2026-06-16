using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScrewDriver.Toolbox.Core.Models;

public class RepairScenario : INotifyPropertyChanged
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public List<string> Commands { get; set; } = new();
    public List<string> DetectCommands { get; set; } = new();

    private string _status = "未检测";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private string _statusColor = "#999999";
    public string StatusColor
    {
        get => _statusColor;
        set { _statusColor = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
