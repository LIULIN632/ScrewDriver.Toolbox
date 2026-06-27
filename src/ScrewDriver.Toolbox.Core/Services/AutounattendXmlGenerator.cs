using System.Text;
using System.Text.Json;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class AutounattendXmlGenerator
{
    public static string Generate(string locale = "zh-CN", string timezone = "China Standard Time",
        bool skipEula = true, bool skipProductKey = true, bool skipOOBE = true,
        bool disableTelemetry = true, bool disableCortana = true, bool disableDefender = false,
        string computerName = "", string userName = "", string userPassword = "")
    {
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<unattend xmlns=\"urn:schemas-microsoft-com:unattend\">");

        // Windows PE 阶段
        sb.AppendLine("  <settings pass=\"windowsPE\">");
        sb.AppendLine("    <component name=\"Microsoft-Windows-International-Core-WinPE\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
        sb.AppendLine($"      <SetupUILanguage><UILanguage>{locale}</UILanguage></SetupUILanguage>");
        sb.AppendLine($"      <InputLocale>{locale}</InputLocale>");
        sb.AppendLine($"      <SystemLocale>{locale}</SystemLocale>");
        sb.AppendLine($"      <UserLocale>{locale}</UserLocale>");
        sb.AppendLine("    </component>");
        sb.AppendLine("    <component name=\"Microsoft-Windows-Setup\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

        if (skipEula)
        {
            sb.AppendLine("      <UserData>");
            sb.AppendLine("        <AcceptEula>true</AcceptEula>");
            sb.AppendLine("      </UserData>");
        }

        if (skipProductKey)
        {
            sb.AppendLine("      <UserData>");
            sb.AppendLine("        <ProductKey><Key>XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</Key></ProductKey>");
            sb.AppendLine("        <AcceptEula>true</AcceptEula>");
            sb.AppendLine("      </UserData>");
        }

        sb.AppendLine("    </component>");
        sb.AppendLine("  </settings>");

        // 专门化阶段
        sb.AppendLine("  <settings pass=\"specialize\">");
        sb.AppendLine("    <component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
        sb.AppendLine($"      <TimeZone>{timezone}</TimeZone>");

        if (!string.IsNullOrEmpty(computerName))
        {
            sb.AppendLine($"      <ComputerName>{computerName}</ComputerName>");
        }

        if (disableTelemetry)
        {
            sb.AppendLine("      <DisableAutoDaylightTimeSet>false</DisableAutoDaylightTimeSet>");
        }

        sb.AppendLine("    </component>");

        if (disableTelemetry)
        {
            sb.AppendLine("    <component name=\"Microsoft-Windows-Deployment\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("      <RunSynchronous>");
            sb.AppendLine("        <RunSynchronousCommand wcm:action=\"add\">");
            sb.AppendLine("          <Order>1</Order>");
            sb.AppendLine("          <Path>reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v AllowTelemetry /t REG_DWORD /d 0 /f</Path>");
            sb.AppendLine("        </RunSynchronousCommand>");
            sb.AppendLine("      </RunSynchronous>");
            sb.AppendLine("    </component>");
        }

        if (disableCortana)
        {
            sb.AppendLine("    <component name=\"Microsoft-Windows-Deployment\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("      <RunSynchronous>");
            sb.AppendLine("        <RunSynchronousCommand wcm:action=\"add\">");
            sb.AppendLine("          <Order>2</Order>");
            sb.AppendLine("          <Path>reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search\" /v AllowCortana /t REG_DWORD /d 0 /f</Path>");
            sb.AppendLine("        </RunSynchronousCommand>");
            sb.AppendLine("      </RunSynchronous>");
            sb.AppendLine("    </component>");
        }

        sb.AppendLine("  </settings>");

        // OOBE 阶段
        if (skipOOBE)
        {
            sb.AppendLine("  <settings pass=\"oobeSystem\">");
            sb.AppendLine("    <component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("      <OOBE>");
            sb.AppendLine("        <HideEULAPage>true</HideEULAPage>");
            sb.AppendLine("        <HideLocalAccountScreen>true</HideLocalAccountScreen>");
            sb.AppendLine("        <HideOEMRegistrationScreen>true</HideOEMRegistrationScreen>");
            sb.AppendLine("        <HideOnlineAccountScreen>true</HideOnlineAccountScreen>");
            sb.AppendLine("        <HideWirelessSetupInOOBE>true</HideWirelessSetupInOOBE>");
            sb.AppendLine("        <NetworkLocation>Work</NetworkLocation>");
            sb.AppendLine("        <ProtectYourPC>1</ProtectYourPC>");
            sb.AppendLine("      </OOBE>");
            sb.AppendLine("      <UserAccounts>");
            sb.AppendLine("        <LocalAccounts>");
            sb.AppendLine("          <LocalAccount wcm:action=\"add\">");
            sb.AppendLine($"            <Name>{userName}</Name>");
            sb.AppendLine("            <Group>Administrators</Group>");
            sb.AppendLine("          </LocalAccount>");
            sb.AppendLine("        </LocalAccounts>");
            sb.AppendLine("      </UserAccounts>");
            sb.AppendLine("    </component>");
            sb.AppendLine("  </settings>");
        }

        // 服务阶段 - 添加自动登录
        sb.AppendLine("  <settings pass=\"specialize\">");
        sb.AppendLine("    <component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
        sb.AppendLine("      <AutoLogon>");
        sb.AppendLine("        <Enabled>true</Enabled>");
        sb.AppendLine("        <Username>Administrator</Username>");
        sb.AppendLine("      </AutoLogon>");
        sb.AppendLine("    </component>");
        sb.AppendLine("  </settings>");

        sb.AppendLine("</unattend>");

        return sb.ToString();
    }

    /// <summary>从预设生成 Autounattend.xml</summary>
    public static string GenerateFromPreset(PresetItem preset, string locale = "zh-CN")
    {
        var states = preset.TargetStates;
        return Generate(
            locale: locale,
            disableTelemetry: states.GetValueOrDefault("telemetry", false),
            disableCortana: states.GetValueOrDefault("disable-copilot", false),
            disableDefender: states.GetValueOrDefault("disable-defender", false)
        );
    }

    /// <summary>生成并保存到文件</summary>
    public static string GenerateToFile(string outputPath)
    {
        var xml = Generate();
        File.WriteAllText(outputPath, xml, Encoding.UTF8);
        return outputPath;
    }
}
