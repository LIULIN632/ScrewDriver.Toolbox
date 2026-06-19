using System;
using System.Runtime.InteropServices;

namespace ScrewDriver.Toolbox.UI.Common;

/// <summary>系统设置变更后刷新 Shell / Explorer</summary>
public static class ShellChangeNotifier
{
    public enum RefreshScope { None, Desktop, Taskbar, AssocChanged, ExplorerRestart }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);

    private const uint WM_SETTINGCHANGE = 0x001A;
    private const uint SMTO_ABORTIFHUNG = 0x0002;
    private const uint SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;

    public static void Notify(RefreshScope scope)
    {
        if (scope == RefreshScope.None) return;

        // 总是广播系统设置变更
        SendMessageTimeout((IntPtr)0xFFFF, WM_SETTINGCHANGE, IntPtr.Zero, "Environment", SMTO_ABORTIFHUNG, 3000, out _);

        if (scope == RefreshScope.AssocChanged)
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        if (scope == RefreshScope.Taskbar)
        {
            SendMessageTimeout((IntPtr)0xFFFF, WM_SETTINGCHANGE, IntPtr.Zero, "TraySettings", SMTO_ABORTIFHUNG, 3000, out _);
        }

        if (scope == RefreshScope.Desktop)
        {
            SendMessageTimeout((IntPtr)0xFFFF, WM_SETTINGCHANGE, IntPtr.Zero, "Policy", SMTO_ABORTIFHUNG, 3000, out _);
        }
    }
}
