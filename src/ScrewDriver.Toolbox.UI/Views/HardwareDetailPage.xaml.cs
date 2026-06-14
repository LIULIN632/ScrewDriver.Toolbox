using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfClipboard = System.Windows.Clipboard;
using WpfCursors = System.Windows.Input.Cursors;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class HardwareDetailPage : Page
{
    public HardwareDetailPage()
    {
        InitializeComponent();
    }

    private void CopyValue(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock tb || string.IsNullOrEmpty(tb.Text))
            return;

        try
        {
            WpfClipboard.SetText(tb.Text);
            var original = tb.Foreground;
            var originalText = tb.Text;
            tb.Text = "✅ 已复制";
            tb.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 101, 52));
            tb.Cursor = WpfCursors.Arrow;
            tb.ToolTip = null;

            var timer = new System.Timers.Timer(1200) { AutoReset = false };
            timer.Elapsed += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    tb.Text = originalText;
                    tb.Foreground = original;
                    tb.Cursor = WpfCursors.Hand;
                    tb.ToolTip = "点击复制";
                });
                timer.Dispose();
            };
            timer.Start();
        }
        catch { }
    }
}
