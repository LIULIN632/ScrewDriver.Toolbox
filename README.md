# ScrewDriver Toolbox

Windows 系统维护工具箱 — 纯 WPF/MVVM，零 NuGet 依赖，Fluent Design 2 风格。

## 功能模块

| 模块 | 说明 |
|------|------|
| **启动** | 常用工具置顶、最近使用、已安装诊断工具 |
| **工具仓库** | 47+ 工具按类别浏览，支持 winget 安装/启动、拖拽添加自定义工具、异步扫描已安装程序 |
| **系统设置 Pro** | ~80 项系统设置（8 类），3 套一键预设（新机推荐/极简模式/老电脑优化），风险标识，搜索过滤，分类恢复 |
| **软件优化** | Edge / Office 等常用软件优化配置 |
| **预设方案** | 创建/编辑/导出/导入预设方案，批量应用 |
| **系统修复** | 6 种修复场景：网络异常、系统卡顿、蓝屏分析、更新失败、游戏掉线、文件异常修复 |
| **硬件检测** | WMI 实时硬件信息，健康状态徽章 |
| **数据中心** | 文件管理中枢，磁盘空间分析 |
| **文件异常修复** | 文件关联检测（注册表扫描 + IFEO 劫持检测）+ 文件占用扫描与解锁（Restart Manager API） |

### 文件异常修复

- **关联检测**：扫描 16 类关键扩展名注册表链（HKCR），校验 exe 存在性、路径合法性、数字签名
- **占用扫描**：基于 Windows Restart Manager API，批量扫描桌面/文档/下载/Temp 目录
- **修复策略**：关联备份→恢复默认；占用优先 `RmShutdown` 温和关闭 → 用户确认后强制结束 → 兜底标记重启删除

## 技术栈

- **运行时**：.NET 10（`net10.0-windows`）
- **UI**：WPF + MVVM（手写，无框架）
- **打包**：`dotnet publish` + Inno Setup 7
- **设计**：Fluent Design 2，深色/浅色主题

## 项目结构

```
src/
├── ScrewDriver.Toolbox.Core/       # 接口、模型、JSON 配置、基础服务
├── ScrewDriver.Toolbox.Utils/      # 通用工具
├── ScrewDriver.Toolbox.Hardware/   # WMI 硬件检测
├── ScrewDriver.Toolbox.SystemTools/# Restart Manager、注册表优化、文件修复
└── ScrewDriver.Toolbox.UI/         # WPF 界面、ViewModel、Converters
```

## 构建

```bash
# Debug
dotnet build ScrewDriver.Toolbox.sln

# Release
dotnet publish src/ScrewDriver.Toolbox.UI -c Release -o publish

# 完整打包（zip + 安装程序）
powershell -ExecutionPolicy Bypass -File build.ps1 -Version "1.0.0"
```

## 安装

1. 下载 `ScrewDriver.Toolbox_Setup_v*_x64.exe`，双击安装
2. 或下载 `ScrewDriver.Toolbox_v*_x64.zip`，解压后运行 `ScrewDriver.Toolbox.exe`

无需管理员权限即可运行（`asInvoker`）。部分功能（系统设置修改、注册表修复）会自动请求提权。
