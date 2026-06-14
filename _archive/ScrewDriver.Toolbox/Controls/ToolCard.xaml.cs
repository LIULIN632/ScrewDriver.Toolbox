using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ScrewDriver.Toolbox.Controls;

public sealed partial class ToolCard : UserControl
{
    public static readonly DependencyProperty ToolNameProperty =
        DependencyProperty.Register(nameof(ToolName), typeof(string), typeof(ToolCard), new PropertyMetadata(""));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(ToolCard), new PropertyMetadata(""));

    public static readonly DependencyProperty VersionProperty =
        DependencyProperty.Register(nameof(Version), typeof(string), typeof(ToolCard), new PropertyMetadata(""));

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(string), typeof(ToolCard), new PropertyMetadata(""));

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(ToolCard), new PropertyMetadata("安装"));

    public string ToolName
    {
        get => (string)GetValue(ToolNameProperty);
        set => SetValue(ToolNameProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Version
    {
        get => (string)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public event RoutedEventHandler? InstallClick;

    public ToolCard()
    {
        InitializeComponent();
    }

    private void OnInstallClick(object sender, RoutedEventArgs e)
    {
        InstallClick?.Invoke(this, e);
    }
}
