using System.Collections.ObjectModel;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.SystemTools.Services;
using ScrewDriver.Toolbox.Core.Services;
using ScrewDriver.Toolbox.UI.Common;
using ScrewDriver.Toolbox.UI.Views.Controls;
using ScrewDriver.Toolbox.UI.Views.Pages;
using WpfApp = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class PresetsViewModel : BaseViewModel
{
    private readonly SystemOptimizerService _service = new();

    public ObservableCollection<PresetItem> PresetList { get; } = new();

    public PresetsViewModel()
    {
        InitPresets();
    }

    private void InitPresets()
    {
        var definitions = _service.GetPresetDefinitions();

        foreach (var def in definitions)
        {
            var item = new PresetItem
            {
                Id = def.Id,
                Name = def.Name,
                Tag = def.Tag,
                Description = def.Description,
                IconCode = def.IconCode,
                Scene = def.Scene,
                Effect = def.Effect,
                Notice = def.Notice,
                SettingKeys = def.TargetStates.Keys.ToList(),
                TargetStates = def.TargetStates
            };
            item.ApplyCommand = new RelayCommand(_ => ApplyPreset(item));
            item.ViewDetailCommand = new RelayCommand(_ => ShowDetail(item));
            item.EditCommand = new RelayCommand(_ => EditPreset(item));
            PresetList.Add(item);
        }
    }

    private async void ApplyPreset(PresetItem preset)
    {
        var total = preset.TargetStates.Count;
        var result = MessageBox.Show(
            $"即将应用预设方案「{preset.Name}」\n\n包含 {total} 项设置\n\n确定继续？",
            "确认应用", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (result != MessageBoxResult.OK) return;

        IsBusy = true;

        var success = 0;
        var fail = 0;
        foreach (var (key, enabled) in preset.TargetStates)
        {
            var ok = await Task.Run(() => RegistryHelper.ApplySettingById(key, enabled));
            if (ok)
                success++;
            else
                fail++;

            await Task.Delay(50);
        }

        IsBusy = false;

        MessageBox.Show(
            $"预设「{preset.Name}」应用完成\n成功 {success} 项 / 失败 {fail} 项\n部分设置需重启系统生效",
            "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowDetail(PresetItem preset)
    {
        var detail = new PresetDetailWindow(preset);
        if (WpfApp.Current.MainWindow is Window main)
            detail.Owner = main;
        detail.ShowDialog();
    }

    private void EditPreset(PresetItem preset)
    {
        var allSettings = _service.GetAllSettings();
        var def = new PresetDefinition
        {
            Id = preset.Id,
            Name = preset.Name,
            Tag = preset.Tag,
            Description = preset.Description,
            IconCode = preset.IconCode,
            Scene = preset.Scene,
            Effect = preset.Effect,
            Notice = preset.Notice,
            TargetStates = new Dictionary<string, bool>(preset.TargetStates)
        };

        var editor = new PresetEditorWindow(def, allSettings);
        if (WpfApp.Current.MainWindow is Window main)
            editor.Owner = main;
        var result = editor.ShowDialog();

        if (result == true)
            RefreshPresets();
    }

    public void RefreshPresets()
    {
        PresetList.Clear();
        InitPresets();
    }

    public void ResetToDefaults()
    {
        PresetStore.ResetToDefaults(() => _service.GetDefaultPresetDefinitions());
        RefreshPresets();
    }
}
