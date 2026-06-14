using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ScrewDriver.Toolbox.Controls;

public sealed partial class StatusCard : UserControl
{
    public static readonly DependencyProperty ComponentNameProperty =
        DependencyProperty.Register(nameof(ComponentName), typeof(string), typeof(StatusCard), new PropertyMetadata(""));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(StatusCard), new PropertyMetadata(""));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(StatusCard), new PropertyMetadata(""));

    public static readonly DependencyProperty StatusDescriptionProperty =
        DependencyProperty.Register(nameof(StatusDescription), typeof(string), typeof(StatusCard), new PropertyMetadata(""));

    public string ComponentName
    {
        get => (string)GetValue(ComponentNameProperty);
        set => SetValue(ComponentNameProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string StatusDescription
    {
        get => (string)GetValue(StatusDescriptionProperty);
        set => SetValue(StatusDescriptionProperty, value);
    }

    public StatusCard()
    {
        InitializeComponent();
    }
}
