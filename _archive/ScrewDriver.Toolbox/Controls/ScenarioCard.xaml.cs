using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ScrewDriver.Toolbox.Controls;

public sealed partial class ScenarioCard : UserControl
{
    public static readonly DependencyProperty ScenarioNameProperty =
        DependencyProperty.Register(nameof(ScenarioName), typeof(string), typeof(ScenarioCard), new PropertyMetadata(""));

    public static readonly DependencyProperty ScenarioDescriptionProperty =
        DependencyProperty.Register(nameof(ScenarioDescription), typeof(string), typeof(ScenarioCard), new PropertyMetadata(""));

    public static readonly DependencyProperty ScenarioIconProperty =
        DependencyProperty.Register(nameof(ScenarioIcon), typeof(string), typeof(ScenarioCard), new PropertyMetadata(""));

    public string ScenarioName
    {
        get => (string)GetValue(ScenarioNameProperty);
        set => SetValue(ScenarioNameProperty, value);
    }

    public string ScenarioDescription
    {
        get => (string)GetValue(ScenarioDescriptionProperty);
        set => SetValue(ScenarioDescriptionProperty, value);
    }

    public string ScenarioIcon
    {
        get => (string)GetValue(ScenarioIconProperty);
        set => SetValue(ScenarioIconProperty, value);
    }

    public event RoutedEventHandler? ExecuteClick;

    public ScenarioCard()
    {
        InitializeComponent();
    }

    private void OnExecuteClick(object sender, RoutedEventArgs e)
    {
        ExecuteClick?.Invoke(this, e);
    }
}
