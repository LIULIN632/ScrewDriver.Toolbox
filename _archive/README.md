# ScrewDriver Toolbox 🪛

**螺丝刀工具箱** — 面向 Windows 用户的可信工具箱。

## 产品定位

集**工具仓库、系统优化、系统修复、硬件检测、品牌工具、场景方案**于一体的 Windows 一站式工具平台。

### 核心理念

- ✅ **绿色透明** — 零广告、零后台常驻、免安装
- ✅ **安全可信** — 工具来源官方/GitHub/Winget/Scoop + SHA 校验
- ✅ **操作可逆** — 所有系统修改支持恢复
- ❌ 不做杀毒、不做管家、不收集隐私

## 技术栈

| 层面 | 技术选型 |
|------|----------|
| 框架 | .NET 8 |
| UI | WinUI 3 (Fluent Design 2) |
| 架构 | MVVM (CommunityToolkit.Mvvm) |
| 依赖注入 | Microsoft.Extensions.DependencyInjection |
| 日志 | Serilog |
| 配置 | JSON |

## 项目结构

```
src/ScrewDriver.Toolbox/
├── App.xaml[.cs]         # 应用入口 + DI 容器
├── AppShell.xaml[.cs]    # NavigationView 主导航
├── Views/                # 页面 (XAML + Code-behind)
│   ├── DashboardPage     # 首页 - 系统概览 + 快捷入口
│   ├── ToolRepositoryPage # 工具仓库 - 搜索 + 分类 + 工具列表
│   ├── SystemOptimizerPage # 系统优化 - 设置开关
│   ├── RepairCenterPage  # 系统修复 - 场景入口 + 解决方案
│   ├── ScenarioPage      # 场景方案 - 一键执行方案
│   ├── HardwareCenterPage # 硬件检测 (占位)
│   ├── BrandToolsPage    # 品牌工具 (占位)
│   ├── DataCenterPage    # 数据中心 (占位)
│   └── SettingsPage      # 设置
├── ViewModels/           # MVVM ViewModel 层
├── Models/               # 数据模型
├── Services/             # 服务层 (接口 + 实现)
├── Controls/             # 可复用 UI 控件
├── Styles/               # 全局样式和主题
├── Helpers/              # 转换器等辅助类
└── Strings/              # 本地化资源
```

## 开发环境要求

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+)
- 工作负载：**.NET 桌面开发** + **Universal Windows Platform 开发**
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/) (1.6+)

## 快速开始

```bash
# 1. 安装 .NET SDK 8.0
# 2. 安装 Windows App SDK 工作负载
dotnet workload install wasdk

# 3. 打开解决方案
cd src
start ScrewDriver.Toolbox.sln

# 或直接编译运行
dotnet build
dotnet run
```

## 模块说明

### MVP (V1.0)
| 模块 | 状态 | 说明 |
|------|------|------|
| 工具仓库 | 🟢 骨架完成 | 搜索 + 分类 + 工具卡片列表 |
| 系统优化 | 🟢 骨架完成 | 分类设置项 + 开关控制 + 还原点 |
| 系统修复 | 🟢 骨架完成 | 问题场景 + 解决方案列表 + 一键执行 |
| 场景方案 | 🟢 骨架完成 | 预设方案卡片 + 步骤展示 |

### V2.0+
| 模块 | 状态 | 说明 |
|------|------|------|
| 硬件检测 | 🟡 占位 | 设备信息 + 温度监控 + 健康建议 |
| 品牌工具 | 🟡 占位 | 品牌识别 + 官方/开源推荐 |
| 数据中心 | 🟡 占位 | 报告 + 备份 + 日志 |

## 设计规范

参考 `工具箱UI设计规范.md` 和 `工具箱WinUI3页面结构规范.md`。

- 遵循 **Microsoft Fluent Design 2**
- 主色：`#0F6CBD`（微软蓝）
- 卡片圆角：12px
- 支持浅色/深色主题
