using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Common;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class DataCenterViewModel : BaseViewModel
{
    private BackupPackage? _selectedBackup;
    private string _reportPreview = string.Empty;
    private string _selectedLogFile = string.Empty;
    private string _logContent = string.Empty;
    private string _logFilter = string.Empty;
    private string _backupRemark = string.Empty;

    public ObservableCollection<BackupPackage> BackupList { get; } = new();
    public ObservableCollection<string> LogFiles { get; } = new();

    public BackupPackage? SelectedBackup
    {
        get => _selectedBackup;
        set => SetProperty(ref _selectedBackup, value);
    }

    public string ReportPreview
    {
        get => _reportPreview;
        set => SetProperty(ref _reportPreview, value);
    }

    public string SelectedLogFile
    {
        get => _selectedLogFile;
        set
        {
            if (SetProperty(ref _selectedLogFile, value))
                LoadLogContent();
        }
    }

    public string LogContent
    {
        get => _logContent;
        set => SetProperty(ref _logContent, value);
    }

    public string LogFilter
    {
        get => _logFilter;
        set
        {
            if (SetProperty(ref _logFilter, value))
                LoadLogContent();
        }
    }

    public string BackupRemark
    {
        get => _backupRemark;
        set => SetProperty(ref _backupRemark, value);
    }

    public ICommand ExportConfigCommand { get; }
    public ICommand ImportConfigCommand { get; }
    public ICommand GenerateAutounattendCommand { get; }
    public ICommand CreateBackupCommand { get; }
    public ICommand RestoreBackupCommand { get; }
    public ICommand DeleteBackupCommand { get; }
    public ICommand ExportBackupCommand { get; }
    public ICommand GenerateTextReportCommand { get; }
    public ICommand GenerateHtmlReportCommand { get; }
    public ICommand CopyReportCommand { get; }
    public ICommand RefreshLogsCommand { get; }

    public DataCenterViewModel()
    {
        ExportConfigCommand = new RelayCommand(_ => ExportConfig());
        ImportConfigCommand = new RelayCommand(_ => ImportConfig());
        GenerateAutounattendCommand = new RelayCommand(_ => GenerateAutounattend());
        CreateBackupCommand = new RelayCommand(_ => CreateBackup());
        RestoreBackupCommand = new RelayCommand(_ => RestoreBackup());
        DeleteBackupCommand = new RelayCommand(_ => DeleteBackup());
        ExportBackupCommand = new RelayCommand(_ => ExportBackup());
        GenerateTextReportCommand = new RelayCommand(_ => GenerateReport("txt"));
        GenerateHtmlReportCommand = new RelayCommand(_ => GenerateReport("html"));
        CopyReportCommand = new RelayCommand(_ => CopyReport());
        RefreshLogsCommand = new RelayCommand(_ => RefreshLogs());

        RefreshBackupList();
        RefreshLogs();
    }

    private void ExportConfig()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "导出系统配置",
            FileName = $"ScrewDriver_Config_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            Filter = "JSON 文件|*.json"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var snapshots = new List<SettingSnapshot>();
                var service = new SystemTools.Services.SystemOptimizerService();
                foreach (var setting in service.GetAllSettings())
                {
                    snapshots.Add(new SettingSnapshot
                    {
                        Key = setting.Id,
                        ValueName = setting.Name,
                        CurrentValue = setting.IsEnabled
                    });
                }
                ConfigManager.ExportConfig(dialog.FileName, snapshots);
                MessageBox.Show($"配置已导出到：\n{dialog.FileName}", "导出成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ImportConfig()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "导入系统配置",
            Filter = "JSON 文件|*.json",
            Multiselect = false
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var result = MessageBox.Show(
                    "导入配置将覆盖当前系统设置，确定继续吗？",
                    "确认导入", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                var config = ConfigManager.LoadConfig(dialog.FileName);
                var success = 0;
                foreach (var (key, enabled) in config)
                {
                    var ok = RegistryHelper.ApplySettingById(key, enabled);
                    if (ok) success++;
                }

                MessageBox.Show($"配置导入完成\n成功应用 {success}/{config.Count} 项设置", "导入完成",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void GenerateAutounattend()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "保存 Autounattend.xml",
            FileName = "Autounattend.xml",
            Filter = "XML 文件|*.xml"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                AutounattendXmlGenerator.GenerateToFile(dialog.FileName);
                MessageBox.Show($"Autounattend.xml 已生成到：\n{dialog.FileName}\n\n请将其放置在 Windows 安装U盘根目录。", "生成成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RefreshBackupList()
    {
        BackupList.Clear();
        foreach (var b in BackupManager.GetBackupList())
            BackupList.Add(b);
    }

    private void CreateBackup()
    {
        var snapshots = new List<SettingSnapshot>();
        var service = new SystemTools.Services.SystemOptimizerService();
        foreach (var setting in service.GetAllSettings())
        {
            snapshots.Add(new SettingSnapshot
            {
                Key = setting.Id,
                RegistryPath = setting.OperationType,
                ValueName = setting.Name,
                OriginalValue = setting.IsEnabled,
                CurrentValue = setting.IsEnabled,
                ModifyTime = DateTime.Now
            });
        }

        var id = BackupManager.CreateFullBackup(
            string.IsNullOrWhiteSpace(BackupRemark) ? "手动备份" : BackupRemark, snapshots);
        BackupRemark = string.Empty;
        OnPropertyChanged(nameof(BackupRemark));
        RefreshBackupList();
        MessageBox.Show($"备份已创建\nID: {id}\n包含 {snapshots.Count} 项设置", "备份完成",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RestoreBackup()
    {
        if (SelectedBackup == null) return;
        var result = MessageBox.Show(
            $"确定要恢复备份「{SelectedBackup.Remark}」吗？\n当前设置将被覆盖。",
            "确认恢复", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        if (BackupManager.RestoreBackup(SelectedBackup.BackupId))
            MessageBox.Show("恢复成功，部分设置可能需要重启资源管理器生效。", "恢复完成",
                MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("恢复失败，备份文件可能已损坏。", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void DeleteBackup()
    {
        if (SelectedBackup == null) return;
        var result = MessageBox.Show(
            $"确定要删除备份「{SelectedBackup.Remark}」吗？此操作不可恢复。",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        BackupManager.DeleteBackup(SelectedBackup.BackupId);
        RefreshBackupList();
    }

    private void ExportBackup()
    {
        if (SelectedBackup == null) return;
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"ScrewDriver_Backup_{SelectedBackup.BackupId}.json",
            Filter = "JSON 文件|*.json"
        };
        if (dialog.ShowDialog() == true)
        {
            var path = BackupManager.ExportBackup(SelectedBackup.BackupId, dialog.FileName);
            if (path != null)
                MessageBox.Show($"备份已导出到：\n{path}", "导出成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("导出失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateReport(string format)
    {
        ReportPreview = format == "html"
            ? SystemReportGenerator.GenerateHtmlReport()
            : SystemReportGenerator.GenerateTextReport();
    }

    private void CopyReport()
    {
        if (string.IsNullOrEmpty(ReportPreview)) return;
        Clipboard.SetText(ReportPreview);
        MessageBox.Show("报告已复制到剪贴板。", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RefreshLogs()
    {
        LogFiles.Clear();
        foreach (var f in Logger.GetLogFiles())
            LogFiles.Add(f);
        if (LogFiles.Count > 0 && string.IsNullOrEmpty(SelectedLogFile))
            SelectedLogFile = LogFiles[0];
    }

    private void LoadLogContent()
    {
        if (string.IsNullOrEmpty(SelectedLogFile))
        {
            LogContent = string.Empty;
            return;
        }

        var content = Logger.ReadLog(SelectedLogFile);
        if (!string.IsNullOrWhiteSpace(LogFilter))
        {
            var lines = content.Split('\n');
            content = string.Join("\n", lines.Where(l =>
                l.Contains(LogFilter, StringComparison.OrdinalIgnoreCase)));
        }
        LogContent = content;
    }
}
