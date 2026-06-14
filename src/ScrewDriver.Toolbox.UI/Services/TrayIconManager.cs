using System.IO;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Icon = System.Drawing.Icon;

namespace ScrewDriver.Toolbox.UI.Services;

public class TrayIconManager : IDisposable
{
    private NotifyIcon? _notifyIcon;
    private readonly Action _showWindow;
    private readonly Action _exitApp;

    public TrayIconManager(Action showWindow, Action exitApp)
    {
        _showWindow = showWindow;
        _exitApp = exitApp;
    }

    public void Initialize()
    {
        var icon = TryLoadAppIcon() ?? SystemIcons.Application;

        _notifyIcon = new NotifyIcon
        {
            Icon = icon,
            Visible = true,
            Text = "ScrewDriver Toolbox"
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("显示主窗口", null, (_, _) => _showWindow());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("退出", null, (_, _) => _exitApp());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => _showWindow();
    }

    private static Icon? TryLoadAppIcon()
    {
        try
        {
            var exePath = Path.Combine(AppContext.BaseDirectory, "ScrewDriver.Toolbox.exe");
            if (File.Exists(exePath))
                return Icon.ExtractAssociatedIcon(exePath);
        }
        catch { }
        return null;
    }

    public void Dispose()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}
