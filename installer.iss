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
; 内置绿色工具（运行 download_tools.ps1 下载后会自动打包）
; 取消下一行注释以强制包含 Tools 目录（即使为空）
; #define IncludeTools
#ifdef IncludeTools
Source: "src\ScrewDriver.Toolbox.UI\Tools\*"; DestDir: "{app}\Tools"; Flags: ignoreversion recursesubdirs createallsubdirs
#endif

[Dirs]
Name: "{app}\Tools"

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
