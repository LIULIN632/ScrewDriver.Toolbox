using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class ScenarioViewModel : BaseViewModel
{
    public ObservableCollection<ToolItem> QuickTools { get; } = new();

    public RelayCommand HostsCommand { get; }
    public RelayCommand KmsCommand { get; }
    public RelayCommand WifiCommand { get; }
    public RelayCommand CertCommand { get; }

    public ScenarioViewModel()
    {
        HostsCommand = new RelayCommand(_ => OpenHosts());
        KmsCommand = new RelayCommand(_ => ShowKmsStatus());
        WifiCommand = new RelayCommand(_ => ShowWifiPasswords());
        CertCommand = new RelayCommand(_ => OpenCertManager());
    }

    private static void OpenHosts()
    {
        try
        {
            var hostsPath = @"C:\Windows\System32\drivers\etc\hosts";
            if (File.Exists(hostsPath))
                Process.Start(new ProcessStartInfo("notepad.exe", hostsPath) { UseShellExecute = true });
            else
                MessageBox.Show("Hosts 文件不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打开失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ShowKmsStatus()
    {
        try
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/k slmgr.vbs -dlv")
            {
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"执行失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ShowWifiPasswords()
    {
        try
        {
            // 获取所有 WiFi 配置文件
            var psi = new ProcessStartInfo("cmd.exe", "/c netsh wlan show profiles")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            // 解析配置文件列表
            var lines = output.Split('\n');
            var profiles = new List<string>();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("所有用户配置文件") || trimmed.StartsWith("    :"))
                    continue;
                if (trimmed.Contains(":"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
                        profiles.Add(parts[1].Trim());
                }
            }

            if (profiles.Count == 0)
            {
                MessageBox.Show("未找到 WiFi 配置文件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 获取每个配置文件的密码
            var sb = new StringBuilder();
            foreach (var profile in profiles)
            {
                var pwdPsi = new ProcessStartInfo("cmd.exe", $"/c netsh wlan show profile \"{profile}\" key=clear")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var pwdP = Process.Start(pwdPsi);
                if (pwdP == null) continue;
                var pwdOutput = pwdP.StandardOutput.ReadToEnd();
                pwdP.WaitForExit();

                var pwd = "无密码";
                foreach (var line2 in pwdOutput.Split('\n'))
                {
                    if (line2.Trim().StartsWith("关键内容") || line2.Trim().StartsWith("Key Content"))
                    {
                        var parts = line2.Split(':');
                        if (parts.Length >= 2)
                            pwd = parts[1].Trim();
                        break;
                    }
                }
                sb.AppendLine($"{profile}: {pwd}");
            }

            MessageBox.Show(sb.ToString(), "WiFi 密码列表",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"获取失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void OpenCertManager()
    {
        try
        {
            Process.Start(new ProcessStartInfo("certmgr.msc") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打开失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
