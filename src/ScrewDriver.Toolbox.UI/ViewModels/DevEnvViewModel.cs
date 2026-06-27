using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class DevEnvViewModel : BaseViewModel
{
    public ObservableCollection<NodeVersionInfo> NodeVersions { get; set; } = new();
    public ObservableCollection<PortInfo> ActivePorts { get; set; } = new();
    public ICommand DownloadCommand { get; }
    public ICommand SwitchCommand { get; }
    public ICommand KillProcessCommand { get; }
    public ICommand RefreshPortsCommand { get; }

    public DevEnvViewModel()
    {
        DownloadCommand = new RelayCommand<string>(DownloadNode);
        SwitchCommand = new RelayCommand<string>(SwitchNode);
        KillProcessCommand = new RelayCommand<int>(KillProcess);
        RefreshPortsCommand = new RelayCommand(_ => LoadPorts());
        _ = LoadNodeVersionsAsync();
        LoadPorts();
    }

    private async Task LoadNodeVersionsAsync()
    {
        try
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://nodejs.org/dist/index.json");
            using var doc = JsonDocument.Parse(json);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                NodeVersions.Clear();
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("version", out var verProp) && verProp.GetString().StartsWith("v18"))
                    {
                        NodeVersions.Add(new NodeVersionInfo
                        {
                            Version = verProp.GetString(),
                            Status = "未安装"
                        });
                    }
                }
            });
        }
        catch { }
    }

    private void LoadPorts()
    {
        ActivePorts.Clear();
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var connections = properties.GetActiveTcpConnections();
        foreach (var conn in connections)
        {
            if (conn.State == TcpState.Listen || conn.State == TcpState.Established)
            {
                ActivePorts.Add(new PortInfo
                {
                    Port = conn.LocalEndPoint.Port,
                    State = conn.State.ToString(),
                    PID = 0
                });
            }
        }
    }

    private void DownloadNode(string version)
    {
        System.Windows.MessageBox.Show($"开始下载 {version}，这里应实现 HttpClient 下载进度和 ZipFile 解压。");
    }

    private void SwitchNode(string version)
    {
        System.Windows.MessageBox.Show($"将环境变量切换至 {version}，需要修改 PATH 并广播 WM_SETTINGCHANGE。");
    }

    private void KillProcess(int pid)
    {
        try
        {
            if (pid > 0)
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
                LoadPorts();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"无法结束进程: {ex.Message}");
        }
    }
}

public class NodeVersionInfo
{
    public string Version { get; set; } = "";
    public string Status { get; set; } = "";
}

public class PortInfo
{
    public int Port { get; set; }
    public string State { get; set; } = "";
    public int PID { get; set; }
}
