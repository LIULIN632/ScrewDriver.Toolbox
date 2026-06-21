using System.Threading;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public static class SettingApplyGuard
{
    private static int _guard;

    public static bool TryEnter() => Interlocked.CompareExchange(ref _guard, 1, 0) == 0;

    public static void Exit() => Interlocked.Exchange(ref _guard, 0);
}
