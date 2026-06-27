using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Services;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class DevEnvViewModel : BaseViewModel
{
    // 环境诊断
    public ObservableCollection<EnvironmentDiagnosticService.DiagnosticItem> Diagnostics { get; } = new();
    
    // 运行时版本
    public ObservableCollection<EnvironmentDiagnosticService.RuntimeInfo> Runtimes { get; } = new();

    // 端口管理
    public ObservableCollection<PortInfo> ActivePorts { get; } = new();

    // 镜像源
    public string CurrentNpmRegistry { get; set; } = "";
    public string CurrentPipSource { get; set; } = "";
    public string CurrentGoProxy { get; set; } = "";

    public ICommand RefreshDiagnosticsCommand { get; }
    public ICommand RefreshPortsCommand { get; }
    public ICommand KillProcessCommand { get; }
    public ICommand SetNpmRegistryCommand { get; }
    public ICommand SetPipSourceCommand { get; }
    public ICommand SetGoProxyCommand { get; }

    public DevEnvViewModel()
    {
        RefreshDiagnosticsCommand = new RelayCommand(_ => LoadDiagnostics());
        RefreshPortsCommand = new RelayCommand(_ => LoadPorts());
        KillProcessCommand = new RelayCommand<int>(KillProcess);
        SetNpmRegistryCommand = new RelayCommand<string>(url => { MirrorConfigService.SetNpmRegistry(url!); CurrentNpmRegistry = MirrorConfigService.GetCurrentNpmRegistry(); });
        SetPipSourceCommand   = new RelayCommand<string>(url => { MirrorConfigService.SetPipSource(url!);   CurrentPipSource  = MirrorConfigService.GetCurrentPipSource(); });
        SetGoProxyCommand     = new RelayCommand<string>(url => { MirrorConfigService.SetGoProxy(url!);     CurrentGoProxy    = MirrorConfigService.GetCurrentGoProxy(); });

        LoadDiagnostics();
        LoadPorts();
        LoadMirrorStatus();
    }

    private void LoadDiagnostics()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Diagnostics.Clear();
            foreach (var item in EnvironmentDiagnosticService.RunDiagnostics())
                Diagnostics.Add(item);
        });

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            Runtimes.Clear();
            foreach (var rt in EnvironmentDiagnosticService.DiscoverRuntimes())
                Runtimes.Add(rt);
        });
    }

    private void LoadPorts()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() => ActivePorts.Clear());
        var ports = new List<PortInfo>();

        // 使用 netstat 获取端口和 PID
        try
        {
            var psi = new ProcessStartInfo("netstat", "-ano")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p != null)
            {
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                foreach (var line in output.Split('\n'))
                {
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5 && parts[0] == "TCP" && parts[3] == "LISTENING")
                    {
                        var portStr = parts[1].Split(':').Last();
                        if (int.TryParse(portStr, out var port) && int.TryParse(parts[4], out var pid))
                        {
                            var processName = "";
                            if (pid > 0)
                            {
                                try { processName = Process.GetProcessById(pid)?.ProcessName ?? ""; } catch { }
                            }
                            ports.Add(new PortInfo { Port = port, State = "LISTEN", PID = pid, ProcessName = processName });
                        }
                    }
                }
            }
        }
        catch { }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var p in ports) ActivePorts.Add(p);
        });
    }

    private void LoadMirrorStatus()
    {
        CurrentNpmRegistry = MirrorConfigService.GetCurrentNpmRegistry();
        CurrentPipSource   = MirrorConfigService.GetCurrentPipSource();
        CurrentGoProxy     = MirrorConfigService.GetCurrentGoProxy();
    }

    private void KillProcess(int pid)
    {
        try
        {
            if (pid <= 0) return;
            var proc = Process.GetProcessById(pid);
            proc.Kill();
            LoadPorts();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"无法结束进程: {ex.Message}");
        }
    }
}

public class PortInfo
{
    public int Port { get; set; }
    public string State { get; set; } = "";
    public int PID { get; set; }
    public string ProcessName { get; set; } = "";
}
