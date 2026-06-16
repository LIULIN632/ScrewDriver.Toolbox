# ScrewDriver.Toolbox build script
param(
    [string]$Config = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0",
    [switch]$Installer = $false
)

$ErrorActionPreference = "Stop"
$RootDir = $PSScriptRoot
$UiProject = "$RootDir\src\ScrewDriver.Toolbox.UI\ScrewDriver.Toolbox.UI.csproj"
$DistDir = "$RootDir\publish_new"
$PublishDir = "$RootDir\src\ScrewDriver.Toolbox.UI\bin\$Config\net10.0-windows\$Runtime\publish"

Write-Host "=== ScrewDriver.Toolbox Build v$Version ===" -ForegroundColor Cyan
Write-Host ""

# 0. Kill old process to avoid file lock
Write-Host "[0/4] Killing old process..." -ForegroundColor Yellow
Get-Process -Name "ScrewDriver.Toolbox" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# 1. Clean
Write-Host "[1/4] Cleaning..." -ForegroundColor Yellow
if (Test-Path $DistDir) { Remove-Item -Recurse -Force $DistDir }
dotnet clean $UiProject -c $Config --verbosity quiet

# 2. Publish
Write-Host "[2/4] Publishing single-file..." -ForegroundColor Yellow
dotnet publish $UiProject -c $Config -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:Version=$Version `
    -p:AssemblyVersion=$Version `
    -p:FileVersion=$Version `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) { Write-Host "Publish failed!" -ForegroundColor Red; exit 1 }

# 3. Package
Write-Host "[3/4] Packaging..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $DistDir | Out-Null

# Copy exe from publish output
if (Test-Path "$PublishDir\ScrewDriver.Toolbox.exe") {
    Copy-Item "$PublishDir\ScrewDriver.Toolbox.exe" "$DistDir\ScrewDriver.Toolbox.exe" -Force
} else {
    # Single-file publish may output directly to DistDir
}

# Create ZIP
$ZipName = "ScrewDriverToolbox_v$Version`_x64.zip"
$ZipPath = "$DistDir\$ZipName"
Compress-Archive -Path "$DistDir\ScrewDriver.Toolbox.exe" -DestinationPath $ZipPath -Force

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Green
Write-Host "EXE : $DistDir\ScrewDriver.Toolbox.exe" -ForegroundColor White
Write-Host "ZIP : $ZipPath" -ForegroundColor White

$exeSize = (Get-Item "$DistDir\ScrewDriver.Toolbox.exe").Length / 1MB
$zipSize = (Get-Item $ZipPath).Length / 1MB
Write-Host "EXE size: $([math]::Round($exeSize, 1)) MB" -ForegroundColor Gray
Write-Host "ZIP size: $([math]::Round($zipSize, 1)) MB" -ForegroundColor Gray

# 4. Installer (optional, requires Inno Setup)
if ($Installer) {
    Write-Host "[4/4] Building installer..." -ForegroundColor Yellow
    $isccPaths = @(
        "$env:ProgramFiles\Inno Setup 7\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    )
    $isccPath = $null
    foreach ($p in $isccPaths) {
        if (Test-Path $p) { $isccPath = $p; break }
    }
    if (Test-Path $isccPath) {
        # 检查 Tools 目录是否有文件，有则启用 IncludeTools
        $toolsDir = "$RootDir\src\ScrewDriver.Toolbox.UI\Tools"
        $hasTools = (Test-Path $toolsDir) -and ((Get-ChildItem $toolsDir -Recurse -File | Measure-Object).Count -gt 0)
        $defineArgs = ""
        if ($hasTools) {
            $defineArgs = "/dIncludeTools"
            Write-Host "  Tools 目录有 $((Get-ChildItem $toolsDir -Recurse -File | Measure-Object).Count) 个文件，已包含" -ForegroundColor Gray
        }
        & $isccPath $defineArgs "$RootDir\installer.iss"
        $setupPath = "$RootDir\installer\ScrewDriverToolbox_v$Version`_Setup.exe"
        if (Test-Path $setupPath) {
            $setupSize = (Get-Item $setupPath).Length / 1MB
            Write-Host "Setup : $setupPath ($([math]::Round($setupSize, 1)) MB)" -ForegroundColor White
        }
    } else {
        Write-Host "  Inno Setup not found. Install with: winget install JRSoftware.InnoSetup" -ForegroundColor Yellow
    }
}
