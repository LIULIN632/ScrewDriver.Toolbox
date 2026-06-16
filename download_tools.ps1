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
