using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.Core.Services;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views.Controls;

public partial class PresetEditorWindow : Window
{
    private readonly PresetDefinition _preset;
    private readonly List<SystemSettingItem> _allSettings;
    private readonly List<PresetSettingItem> _settingItems = new();

    public PresetEditorWindow(PresetDefinition preset, List<SystemSettingItem> allSettings)
    {
        InitializeComponent();
        _preset = preset;
        _allSettings = allSettings;
        LoadMetadata();
        BuildSettingsList();
    }

    private void LoadMetadata()
    {
        TxtName.Text = _preset.Name;
        TxtTag.Text = _preset.Tag;
        TxtDesc.Text = _preset.Description;
        TxtIcon.Text = _preset.IconCode;
        TxtScene.Text = _preset.Scene;
        TxtEffect.Text = _preset.Effect;
        TxtNotice.Text = _preset.Notice;
    }

    private void BuildSettingsList()
    {
        var groups = new ObservableCollection<CategoryGroup>();

        foreach (var g in _allSettings.GroupBy(s => s.Category))
        {
            var group = new CategoryGroup { CategoryName = g.Key };
            foreach (var setting in g)
            {
                var targetEnabled = _preset.TargetStates.TryGetValue(setting.Id, out var target) && target;
                var item = new PresetSettingItem
                {
                    Id = setting.Id,
                    Name = setting.Name,
                    Description = setting.Description,
                    IsIncluded = _preset.TargetStates.ContainsKey(setting.Id),
                    TargetEnabled = targetEnabled
                };
                item.PropertyChanged += OnSettingItemChanged;
                _settingItems.Add(item);
                group.Items.Add(item);
            }
            groups.Add(group);
        }

        SettingsList.ItemsSource = groups;
    }

    private void OnSettingItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not PresetSettingItem item) return;
        if (e.PropertyName == nameof(PresetSettingItem.IsIncluded))
        {
            if (item.IsIncluded)
                item.TargetEnabled = true;
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("请输入预设名称。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _preset.Name = TxtName.Text.Trim();
        _preset.Tag = TxtTag.Text.Trim();
        _preset.Description = TxtDesc.Text.Trim();
        _preset.IconCode = TxtIcon.Text.Trim();
        _preset.Scene = TxtScene.Text.Trim();
        _preset.Effect = TxtEffect.Text.Trim();
        _preset.Notice = TxtNotice.Text.Trim();

        _preset.TargetStates.Clear();
        foreach (var item in _settingItems)
        {
            if (item.IsIncluded)
                _preset.TargetStates[item.Id] = item.TargetEnabled;
        }

        PresetStore.SavePreset(_preset);
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public class PresetSettingItem : INotifyPropertyChanged
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    private bool _isIncluded;
    public bool IsIncluded
    {
        get => _isIncluded;
        set { if (_isIncluded == value) return; _isIncluded = value; OnPropertyChanged(); OnPropertyChanged(nameof(TargetEnabled)); }
    }

    private bool _targetEnabled;
    public bool TargetEnabled
    {
        get => _targetEnabled;
        set { if (_targetEnabled == value) return; _targetEnabled = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class CategoryGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public ObservableCollection<PresetSettingItem> Items { get; } = new();
}
