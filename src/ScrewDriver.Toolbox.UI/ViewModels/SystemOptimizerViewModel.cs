using System.Collections.ObjectModel;
using System.Windows;
using ScrewDriver.Toolbox.Core.Interfaces;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.SystemTools.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class BloatwareItem : BaseViewModel
{
    public string PackageName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public class SystemOptimizerViewModel : BaseViewModel
{
    private readonly ISystemOptimizerService _service = new SystemOptimizerService();
    private readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);
    private bool _isUninstalling;
    private string _uninstallStatus = string.Empty;

    private class PersistedSettings { public List<string> EnabledIds { get; set; } = new(); }

    public ObservableCollection<SettingGroup> GroupedSettings { get; } = new();
    public ObservableCollection<BloatwareItem> BloatwareList { get; } = new();

    public bool IsUninstalling
    {
        get => _isUninstalling;
        set => SetProperty(ref _isUninstalling, value);
    }

    public string UninstallStatus
    {
        get => _uninstallStatus;
        set => SetProperty(ref _uninstallStatus, value);
    }

    public RelayCommand UninstallSelectedBloatwareCommand { get; }
    public RelayCommand SelectAllBloatwareCommand { get; }
    public RelayCommand DeselectAllBloatwareCommand { get; }
    public RelayCommand RevertGroupCommand { get; }

    public SystemOptimizerViewModel()
    {
        UninstallSelectedBloatwareCommand = new RelayCommand(async _ => await UninstallSelectedBloatwareAsync());
        SelectAllBloatwareCommand = new RelayCommand(_ => SetAllBloatware(true));
        DeselectAllBloatwareCommand = new RelayCommand(_ => SetAllBloatware(false));
        RevertGroupCommand = new RelayCommand(param => RevertGroup(param as string ?? ""));
        LoadSettings();
    }

    public void LoadSettings()
    {
        GroupedSettings.Clear();
        var all = _service.GetAllSettings();

        var currentPlan = _service.GetCurrentPowerPlan();
        if (currentPlan != null)
        {
            var plan = all.FirstOrDefault(s => s.Id == currentPlan);
            if (plan != null) plan.IsEnabled = true;
        }

        // Restore persisted user choices (overrides registry-read default)
        var persisted = _config.Load<PersistedSettings>("system-settings") ?? new PersistedSettings();
        foreach (var id in persisted.EnabledIds)
        {
            var item = all.FirstOrDefault(s => s.Id == id);
            if (item != null) item.IsEnabled = true;
        }

        var groups = all.GroupBy(s => s.Category).OrderBy(g => g.Key);
        foreach (var group in groups)
        {
            GroupedSettings.Add(new SettingGroup
            {
                CategoryName = group.Key,
                Items = new ObservableCollection<SystemSettingItem>(group)
            });
        }

        BloatwareList.Clear();
        foreach (var (name, desc) in SystemOptimizerService.BloatwarePackages)
        {
            BloatwareList.Add(new BloatwareItem
            {
                PackageName = name,
                DisplayName = desc,
                IsSelected = true
            });
        }
    }

    private void SavePersistedSettings()
    {
        var ids = GroupedSettings
            .SelectMany(g => g.Items)
            .Where(s => s.IsEnabled)
            .Select(s => s.Id)
            .ToList();
        _config.Save("system-settings", new PersistedSettings { EnabledIds = ids });
    }

    public bool ApplySetting(string id, bool enable)
    {
        if (id.StartsWith("power-plan-") && enable)
        {
            foreach (var group in GroupedSettings)
            {
                var powerPlans = group.Items.Where(i => i.Id.StartsWith("power-plan-") && i.Id != id).ToList();
                foreach (var plan in powerPlans)
                    plan.IsEnabled = false;
            }
        }

        var ok = _service.ApplySetting(id, enable);
        if (ok) SavePersistedSettings();
        return ok;
    }

    public void RevertAll()
    {
        foreach (var group in GroupedSettings)
        foreach (var setting in group.Items)
        {
            if (setting.IsEnabled)
            {
                _service.RevertSetting(setting.Id);
                setting.IsEnabled = false;
            }
        }
        SavePersistedSettings();
    }

    private void RevertGroup(string categoryName)
    {
        var group = GroupedSettings.FirstOrDefault(g => g.CategoryName == categoryName);
        if (group == null) return;

        var result = MessageBox.Show(
            $"确定要将「{categoryName}」分组的所有设置恢复为默认值吗？",
            "恢复本组默认", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        foreach (var setting in group.Items)
        {
            if (setting.IsEnabled)
            {
                _service.RevertSetting(setting.Id);
                setting.IsEnabled = false;
            }
        }
        SavePersistedSettings();
    }

    private void SetAllBloatware(bool selected)
    {
        foreach (var item in BloatwareList)
            item.IsSelected = selected;
    }

    private async Task UninstallSelectedBloatwareAsync()
    {
        var selected = BloatwareList.Where(b => b.IsSelected).Select(b => b.PackageName).ToArray();
        if (selected.Length == 0)
        {
            MessageBox.Show("请至少选择一项要卸载的应用。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var names = BloatwareList.Where(b => b.IsSelected).Select(b => b.DisplayName).ToList();
        var preview = string.Join("、", names.Take(8));
        if (names.Count > 8) preview += $" 等{names.Count}项";

        var result = MessageBox.Show(
            $"即将卸载以下预装应用：\n\n{preview}\n\n此操作不可自动恢复，确定继续？",
            "卸载预装应用", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        IsUninstalling = true;

        var progress = new Progress<(string status, int progress)>(report =>
        {
            UninstallStatus = report.status;
        });

        await Task.Run(() => _service.UninstallBloatwareAsync(progress, selected));

        IsUninstalling = false;
        UninstallStatus = string.Empty;

        MessageBox.Show("所选预装应用卸载完成！", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
