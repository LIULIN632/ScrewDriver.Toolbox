<#
.SYNOPSIS
    下载绿色版工具到 Tools 目录，打包进安装包
.DESCRIPTION
    从各工具官网下载便携版，放入 src/ScrewDriver.Toolbox.UI/Tools/
    然后运行 .\build.ps1 -Installer 即可打包
#>

$ToolsDir = "src\ScrewDriver.Toolbox.UI\Tools"
New-Item -ItemType Directory -Force -Path $ToolsDir | Out-Null

$tools = @(
    @{ Name = "Everything"; Url = "https://www.voidtools.com/Everything-1.4.1.1026.x64.zip"; Exe = "Everything.exe" },
    @{ Name = "Dism++"; Url = "https://github.com/Chuyu-Team/Dism-Multi-language/releases/download/v10.1.1002.1/Dism++10.1.1002.1.zip"; Exe = "Dism++x64.exe" },
    @{ Name = "WizTree"; Url = "https://diskanalyzer.com/files/WizTree_x64_portable.zip"; Exe = "WizTree64.exe" },
    @{ Name = "Ventoy"; Url = "https://github.com/ventoy/Ventoy/releases/download/v1.1.05/ventoy-1.1.05-windows.zip"; Exe = "Ventoy2Disk.exe" },
    @{ Name = "HiBit Uninstaller"; Url = "https://www.hibitsoft.ir/HiBitUninstaller/HiBitUninstaller-Portable.zip"; Exe = "HiBitUninstaller.exe" },
    @{ Name = "CPU-Z"; Url = "https://download.cpuid.com/cpu-z/cpu-z_2-en.zip"; Exe = "cpuz_x64.exe" },
    @{ Name = "GPU-Z"; Url = "https://www.techpowerup.com/download/gpu-z/"; Exe = "GPU-Z.exe" },
    @{ Name = "CrystalDiskInfo"; Url = "https://crystalmark.info/download/CrystalDiskInfo.zip"; Exe = "DiskInfo64.exe" },
    @{ Name = "CrystalDiskMark"; Url = "https://crystalmark.info/download/CrystalDiskMark.zip"; Exe = "DiskMark64.exe" },
    @{ Name = "HWiNFO"; Url = "https://www.hwinfo.com/download/hwi64_753.exe"; Exe = "HWiNFO64.exe" }
)

foreach ($tool in $tools) {
    $name = $tool.Name
    $url = $tool.Url
    $outDir = "$ToolsDir\$name"
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
    $outFile = "$outDir\tool.zip"
    
    Write-Host "下载 $name ..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Uri $url -OutFile $outFile -UseBasicParsing
        Expand-Archive -Path $outFile -DestinationPath $outDir -Force
        Remove-Item $outFile
        Write-Host "  $name 下载完成" -ForegroundColor Green
    }
    catch {
        Write-Host "  $name 下载失败: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== 下载完成 ===" -ForegroundColor Green
Write-Host "工具已保存到 $ToolsDir"
Write-Host "运行 .\build.ps1 -Installer 打包安装包" -ForegroundColor Cyan
