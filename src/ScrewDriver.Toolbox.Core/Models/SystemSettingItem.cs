using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScrewDriver.Toolbox.Core.Interfaces;

namespace ScrewDriver.Toolbox.Core.Models;

public enum RiskLevel
{
    Recommended,
    Optional,
    Dangerous
}

public enum RecommendedAction
{
    None,
    Enable,
    Disable
}

public enum SettingControlType
{
    Toggle,
    ComboBox,
    RadioGroup,
    ActionButton
}

public class SystemSettingItem : IRiskOperation, INotifyPropertyChanged
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    public void IsEnabledSilent(bool value)
    {
        if (_isEnabled == value) return;
        _isEnabled = value;
    }
    public bool IsDangerous => RiskLevel == RiskLevel.Dangerous;
    public string StatusText => IsEnabled ? "已开启" : "已关闭";
    public string StatusColor => IsEnabled ? "#22C55E" : "#999999";

    public string RiskLabel => RiskLevel switch
    {
        RiskLevel.Recommended => "安全",
        RiskLevel.Optional => "高级",
        RiskLevel.Dangerous => "危险",
        _ => ""
    };

    public string RiskBadgeColor => RiskLevel switch
    {
        RiskLevel.Recommended => "#22C55E",
        RiskLevel.Optional => "#F59E0B",
        RiskLevel.Dangerous => "#EF4444",
        _ => "#999999"
    };

    public string RecommendationBadgeColor => Recommendation switch
    {
        RecommendedAction.Enable => "#22C55E",
        RecommendedAction.Disable => "#EF4444",
        _ => "#999999"
    };

    public string RiskDescription { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public RecommendedAction Recommendation { get; set; }
    public string RecommendationText => Recommendation switch
    {
        RecommendedAction.Enable => "推荐开启",
        RecommendedAction.Disable => "推荐关闭",
        _ => ""
    };
    public bool CanRevert { get; set; } = true;
    public string RevertMethodDescription { get; set; } = string.Empty;
    public string OperationType { get; set; } = "Registry";
    public string? EnablePsCommand { get; set; }
    public string? DisablePsCommand { get; set; }
    public SettingControlType ControlType { get; set; } = SettingControlType.Toggle;
    public ObservableCollection<string> Options { get; set; } = new();
    public string? ActionCommand { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class SettingGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public ObservableCollection<SystemSettingItem> Items { get; set; } = new();
}
