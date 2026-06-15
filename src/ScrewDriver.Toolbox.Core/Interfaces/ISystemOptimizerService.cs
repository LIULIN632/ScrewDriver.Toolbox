using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Interfaces;

public interface ISystemOptimizerService
{
    List<SystemSettingItem> GetAllSettings();
    bool ApplySetting(string id, bool enable);
    bool RevertSetting(string id);
    Task UninstallBloatwareAsync(IProgress<(string status, int progress)>? progress = null, string[]? selectedPackages = null);
    Task<List<string>> CheckInstalledBloatwareAsync(string[] packageNames);
    string? GetCurrentPowerPlan();
}
