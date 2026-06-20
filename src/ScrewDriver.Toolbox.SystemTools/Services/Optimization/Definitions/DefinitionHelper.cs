using Microsoft.Win32;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.SystemTools.Services.Optimization.Definitions;

using SettingDef = (string Id, string Name, string Desc, string Cat, RiskLevel Risk, string RiskDesc, string Revert,
    RegistryHive Hive, string KeyPath, string ValueName, object EnabledVal, object? DisabledVal, RegistryValueKind Kind,
    string OperationType, string? EnablePsCmd, string? DisablePsCmd);

internal static class DefinitionHelper
{
    public static SettingDef RegToggle(
        string id, string name, string desc, string cat, RiskLevel risk, string riskDesc, string revert,
        RegistryHive hive, string keyPath, string valueName,
        object enabledVal = null!, object? disabledVal = null)
    {
        var ev = enabledVal ?? 1;
        var dv = disabledVal ?? 0;
        return (id, name, desc, cat, risk, riskDesc, revert,
            hive, keyPath, valueName, ev, dv, RegistryValueKind.DWord,
            "Registry", null, null);
    }
}
