using Microsoft.Win32;

namespace ScrewDriver.Toolbox.Core.Services;

public enum AppTheme { Light, Dark, System }

public static class ThemeService
{
    private static AppTheme _currentTheme = AppTheme.System;
    private static readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);

    public static AppTheme CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                ThemeChanged?.Invoke(null, value);
            }
        }
    }

    public static event EventHandler<AppTheme>? ThemeChanged;

    public static void Initialize()
    {
        var saved = _config.Load<ThemeConfig>("theme");
        if (saved != null && Enum.TryParse<AppTheme>(saved.Theme, out var parsed))
            _currentTheme = parsed;
        else
            _currentTheme = AppTheme.Light;

        SystemEvents.UserPreferenceChanged += (_, e) =>
        {
            if (_currentTheme == AppTheme.System && e.Category == UserPreferenceCategory.General)
                ThemeChanged?.Invoke(null, AppTheme.System);
        };
    }

    public static void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        _config.Save("theme", new ThemeConfig { Theme = theme.ToString() });
    }

    public static bool IsDarkMode()
    {
        if (_currentTheme == AppTheme.Dark) return true;
        if (_currentTheme == AppTheme.Light) return false;
        return SystemUsesDarkMode();
    }

    private static bool SystemUsesDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
        }
        catch { return false; }
    }

    private class ThemeConfig { public string Theme { get; set; } = "System"; }
}
