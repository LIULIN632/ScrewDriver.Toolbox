using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScrewDriver.Toolbox.UI.ViewModels;
using WpfClipboard = System.Windows.Clipboard;
using WpfCursors = System.Windows.Input.Cursors;

namespace ScrewDriver.Toolbox.UI.Views.Pages;

public partial class HardwarePage : System.Windows.Controls.UserControl
{
    public HardwarePage()
    {
        InitializeComponent();
        Loaded += (_, _) => (DataContext as HardwareViewModel)?.Activate();
        Unloaded += (_, _) => (DataContext as HardwareViewModel)?.Deactivate();
    }

    private void CopyToClipboard(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock textBlock || string.IsNullOrEmpty(textBlock.Text))
            return;

        try
        {
            WpfClipboard.SetText(textBlock.Text);
            CopyFeedback(textBlock);
        }
        catch { /* clipboard access denied */ }
    }

    private static async void CopyFeedback(TextBlock tb)
    {
        var originalText = tb.Text;
        var originalForeground = tb.Foreground;
        tb.Text = "✅ 已复制";
        tb.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 101, 52)); // 绿色
        tb.Cursor = WpfCursors.Arrow;
        tb.ToolTip = null;

        await Task.Delay(1200);

        tb.Text = originalText;
        tb.Foreground = originalForeground;
        tb.Cursor = WpfCursors.Hand;
        tb.ToolTip = "点击复制";
    }
}
