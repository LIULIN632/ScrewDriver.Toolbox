param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = "",
    [string]$ISCC = ""
)

$ErrorActionPreference = "Stop"
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

if ($OutputDir -eq "") {
    $OutputDir = Join-Path $ScriptRoot "dist"
}
$BuildTemp = Join-Path $ScriptRoot "build_temp"
$PublishTemp = Join-Path $ScriptRoot "publish_temp"
$ProjectPath = Join-Path $ScriptRoot "src\ScrewDriver.Toolbox.UI\ScrewDriver.Toolbox.UI.csproj"

if ($ISCC -eq "") {
    $ISCC = "C:\Program Files\Inno Setup 7\ISCC.exe"
    if (-not (Test-Path $ISCC)) {
        $ISCC = "C:\Users\24721\AppData\Local\Programs\Inno Setup 6\ISCC.exe"
    }
}

Write-Host "=== ScrewDriver Toolbox Build Script v$Version ===" -ForegroundColor Cyan

# Step 1: Clean
Write-Host "[1/5] Cleaning old output..." -ForegroundColor Yellow
@($OutputDir, $BuildTemp, $PublishTemp, (Join-Path $ScriptRoot "installer")) | ForEach-Object {
    if (Test-Path $_) { Remove-Item -Recurse -Force $_ -ErrorAction SilentlyContinue }
}
New-Item -ItemType Directory -Force $OutputDir | Out-Null

# Step 2: Build + Publish
Write-Host "[2/5] Building and publishing..." -ForegroundColor Yellow
dotnet publish $ProjectPath -c Release -o $PublishTemp
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# Step 3: Zip
Write-Host "[3/5] Creating zip archive..." -ForegroundColor Yellow
$ZipName = "ScrewDriver.Toolbox_v$($Version)_x64.zip"
$ZipPath = Join-Path $OutputDir $ZipName
Compress-Archive -LiteralPath $PublishTemp -DestinationPath $ZipPath -CompressionLevel Optimal -Force
Write-Host "  -> $ZipPath" -ForegroundColor Green

# Step 4: Inno Setup
Write-Host "[4/5] Creating installer..." -ForegroundColor Yellow
if (-not (Test-Path $ISCC)) {
    Write-Warning "Inno Setup not found, skipping installer"
} else {
    $SetupName = "ScrewDriver.Toolbox_Setup_v$($Version)_x64"
    $IssContent = @"
#define MyAppName "ScrewDriver Toolbox"
#define MyAppVersion "$Version"
#define MyAppPublisher "ScrewDriver"
#define MyAppExeName "ScrewDriver.Toolbox.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=$ScriptRoot\installer
OutputBaseFilename=$SetupName
SetupIconFile=$ScriptRoot\src\ScrewDriver.Toolbox.UI\app.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

[Files]
Source: "$PublishTemp\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
"@
    $IssFile = Join-Path $BuildTemp "setup.iss"
    New-Item -ItemType Directory -Force $BuildTemp | Out-Null
    $IssContent | Set-Content -Path $IssFile -Encoding UTF8

    & $ISCC $IssFile
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup compile failed" }

    $SetupExe = Get-ChildItem (Join-Path $ScriptRoot "installer") -Filter "*.exe" | Select-Object -First 1
    Copy-Item $SetupExe.FullName $OutputDir -Force
    Write-Host "  -> $OutputDir\$($SetupExe.Name)" -ForegroundColor Green
}

# Step 5: Cleanup
Write-Host "[5/5] Cleaning up temp files..." -ForegroundColor Yellow
@($BuildTemp, $PublishTemp, (Join-Path $ScriptRoot "installer")) | ForEach-Object {
    if (Test-Path $_) { Remove-Item -Recurse -Force $_ -ErrorAction SilentlyContinue }
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
Get-ChildItem $OutputDir | ForEach-Object {
    $size = "{0:N1} MB" -f ($_.Length / 1MB)
    Write-Host "  $($_.Name)  ($size)" -ForegroundColor White
}
