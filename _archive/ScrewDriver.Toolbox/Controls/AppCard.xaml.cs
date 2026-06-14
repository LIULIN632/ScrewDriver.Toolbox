using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ScrewDriver.Toolbox.Controls;

public sealed partial class AppCard : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(AppCard), new PropertyMetadata(""));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(AppCard), new PropertyMetadata(""));

    public static readonly DependencyProperty ActionTextProperty =
        DependencyProperty.Register(nameof(ActionText), typeof(string), typeof(AppCard), new PropertyMetadata("打开"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public event RoutedEventHandler? ActionClick;

    public AppCard()
    {
        InitializeComponent();
    }

    private void OnActionClick(object sender, RoutedEventArgs e)
    {
        ActionClick?.Invoke(this, e);
    }
}
