using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ScrewDriver.Toolbox.Core.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI;

public partial class App : System.Windows.Application
{
    private static bool _handlingException;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
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
        catch (Exception ex)
        {
            LogCrash("OnStartup", ex);
        }
    }

    private static void ApplyCurrentTheme()
    {
        // Theme files (LightTheme.xaml / DarkTheme.xaml) to be added in future.
        // Current colors are defined in App.xaml MergedDictionaries (Colors.xaml + Styles.xaml).
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        if (_handlingException) return;
        _handlingException = true;
        try
        {
            LogCrash("DispatcherUI", e.Exception);
        }
        finally { _handlingException = false; }
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogCrash("AppDomain", ex);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogCrash("Task", e.Exception);
        e.SetObserved();
    }

    internal static void LogCrash(string source, Exception ex)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ScrewDriverToolbox", "Logs");
            Directory.CreateDirectory(dir);
            var logPath = Path.Combine(dir, $"crash_{DateTime.Now:yyyyMMdd}.log");
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}\n";
            File.AppendAllText(logPath, entry);
            Debug.WriteLine($"[Crash] [{source}] {ex.Message}");
        }
        catch { /* 最后保障：日志失败不抛异常 */ }
    }
}
