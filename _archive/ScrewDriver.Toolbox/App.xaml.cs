using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ScrewDriver.Toolbox.Services;
using ScrewDriver.Toolbox.ViewModels;
using ScrewDriver.Toolbox.Views;
using Serilog;
using System;

namespace ScrewDriver.Toolbox;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        InitializeComponent();

        _host = CreateHostBuilder().Build();
    }

    public static T GetService<T>() where T : class
    {
        if ((Application.Current as App)!._host.Services.GetService(typeof(T)) is not T service)
            throw new InvalidOperationException($"Service {typeof(T)} not registered.");
        return service;
    }

    private static IHost CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureLogging(logging =>
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File("Logs/toolbox.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            })
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IDeviceInfoService, DeviceInfoService>();
                services.AddSingleton<IToolCatalogService, ToolCatalogService>();
                services.AddSingleton<ISystemOptimizerService, SystemOptimizerService>();
                services.AddSingleton<ISystemRepairService, SystemRepairService>();

                // ViewModels
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ToolRepositoryViewModel>();
                services.AddTransient<SystemOptimizerViewModel>();
                services.AddTransient<RepairCenterViewModel>();
                services.AddTransient<HardwareCenterViewModel>();
                services.AddTransient<BrandToolsViewModel>();
                services.AddTransient<ScenarioViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<DataCenterViewModel>();

                // Pages
                services.AddTransient<DashboardPage>();
                services.AddTransient<ToolRepositoryPage>();
                services.AddTransient<SystemOptimizerPage>();
                services.AddTransient<RepairCenterPage>();
                services.AddTransient<HardwareCenterPage>();
                services.AddTransient<BrandToolsPage>();
                services.AddTransient<ScenarioPage>();
                services.AddTransient<SettingsPage>();
                services.AddTransient<DataCenterPage>();
            })
            .Build();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var mainWindow = new MainWindow();
        mainWindow.Activate();
    }
}

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "ScrewDriver Toolbox";
        ExtendsContentIntoTitleBar = true;
        Content = new AppShell();
    }
}
