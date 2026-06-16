; ScrewDriver Toolbox Installer
; Requires Inno Setup 6+ (https://jrsoftware.org/isdl.php)
; Build: iscc installer.iss

#define MyAppName "ScrewDriver Toolbox"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ScrewDriver"
#define MyAppURL "https://github.com/ScrewDriver/Toolbox"
#define MyAppExeName "ScrewDriver.Toolbox.exe"

[Setup]
AppId={{B8A3C5E1-2F4D-4A6B-9C7D-8E5F1A3B2C4D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.\installer
OutputBaseFilename=ScrewDriverToolbox_v{#MyAppVersion}_Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "dist\release\ScrewDriver.Toolbox.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeUninstall(): Boolean;
begin
  Result := True;
end;
