using System.Windows;
using System.Windows.Media;
using ScrewDriver.Toolbox.Core.Services;

namespace ScrewDriver.Toolbox.UI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ThemeService.Initialize();
        ApplyCurrentTheme();

        ThemeService.ThemeChanged += (_, _) =>
        {
            Dispatcher.Invoke(ApplyCurrentTheme);
        };

        var window = new MainWindow();
        window.Show();
    }

    private static void ApplyCurrentTheme()
    {
        var isDark = ThemeService.IsDarkMode();
        var themeName = isDark ? "DarkTheme.xaml" : "LightTheme.xaml";
        var uri = new Uri($"pack://application:,,,/Themes/{themeName}");
        var dict = new ResourceDictionary { Source = uri };

        foreach (var key in dict.Keys)
        {
            if (dict[key] is SolidColorBrush themeBrush &&
                Current.Resources.Contains(key))
            {
                Current.Resources[key] = new SolidColorBrush(themeBrush.Color);
            }
        }
    }
}
