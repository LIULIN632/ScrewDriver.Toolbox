using ScrewDriver.Toolbox.Models;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.Services;

/// <summary>
/// 系统优化服务
/// </summary>
public interface ISystemOptimizerService
{
    ObservableCollection<SettingItem> GetSettingsByCategory(string category);
    ObservableCollection<string> GetCategories();
    bool ApplySetting(SettingItem item, bool enable);
    bool RestoreSetting(string settingId);
    bool CreateRestorePoint();
}
