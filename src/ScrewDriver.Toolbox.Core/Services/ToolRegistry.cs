using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class ToolRegistry
{
    private static List<ToolItem>? _allTools;
    private static List<ToolItem>? _customTools;
    private static readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);

    public static List<string> Categories { get; } = new()
    {
        "系统工具", "CPU工具", "显卡工具", "内存工具",
        "磁盘工具", "屏幕工具", "综合检测", "网络工具",
        "外设工具", "游戏工具", "其他工具", "品牌工具"
    };

    public static List<ToolItem> GetAllTools()
    {
        if (_allTools == null)
        {
            _allTools = new List<ToolItem>();
            AddSystemTools();
            AddCpuTools();
            AddGpuTools();
            AddMemoryTools();
            AddDiskTools();
            AddScreenTools();
            AddComprehensiveTools();
            AddNetworkTools();
            AddPeripheralTools();
            AddGameTools();
            AddOtherTools();
            AddBrandTools();
        }
        var all = new List<ToolItem>(_allTools);
        all.AddRange(GetCustomTools());
        return all;
    }

    public static List<ToolItem> GetCustomTools()
    {
        if (_customTools == null)
            _customTools = _config.Load<List<ToolItem>>("custom-tools") ?? new List<ToolItem>();
        return _customTools;
    }

    public static void AddCustomTool(ToolItem tool)
    {
        var tools = GetCustomTools();
        tools.Add(tool);
        SaveCustomTools();
    }

    public static void RemoveCustomTool(ToolItem tool)
    {
        var tools = GetCustomTools();
        tools.Remove(tool);
        SaveCustomTools();
    }

    private static void SaveCustomTools()
    {
        _config.Save("custom-tools", _customTools!);
    }

    // ============================================================
    // 1. 系统工具 (7)
    // ============================================================
    private static void AddSystemTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Everything", Category = "系统工具",
            Description = "极速文件搜索，秒级索引全盘文件，远超系统自带搜索",
            OfficialUrl = "https://www.voidtools.com/",
            WingetId = "voidtools.Everything", RiskLevel = "推荐" },
        new() { Name = "7-Zip", Category = "系统工具",
            Description = "开源高压缩比解压工具，支持 7z/XZ/BZIP2/ZIP/RAR 等格式",
            OfficialUrl = "https://7-zip.org/",
            WingetId = "7zip.7zip", RiskLevel = "推荐" },
        new() { Name = "Geek Uninstaller", Category = "系统工具",
            Description = "轻量级软件卸载工具，深度扫描残留文件和注册表项",
            OfficialUrl = "https://geekuninstaller.com/",
            WingetId = "GeekUninstaller.GeekUninstaller", RiskLevel = "安全" },
        new() { Name = "Dism++", Category = "系统工具",
            Description = "系统清理、映像管理、更新管理与 Windows 优化一体化工具",
            OfficialUrl = "https://github.com/Chuyu-Team/Dism-Multi-language",
            WingetId = "Dism++.Dism++", RiskLevel = "注意" },
        new() { Name = "WizTree", Category = "系统工具",
            Description = "极速磁盘空间分析，MFT 直读秒出结果，直观定位大文件",
            OfficialUrl = "https://diskanalyzer.com/",
            WingetId = "AntibodySoftware.WizTree", RiskLevel = "推荐" },
        new() { Name = "DirectX 修复工具", Category = "系统工具",
            Description = "DirectX 组件修复与运行库补全，一键检测并修复缺失 DLL",
            OfficialUrl = "https://www.crsky.com/soft/77560.html",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "BlueScreenView", Category = "系统工具",
            Description = "NirSoft 蓝屏转储分析，自动解析 minidump 文件定位崩溃驱动",
            OfficialUrl = "https://www.nirsoft.net/utils/blue_screen_view.html",
            WingetId = "NirSoft.BlueScreenView", RiskLevel = "安全" }
    });

    // ============================================================
    // 2. CPU工具 (6)
    // ============================================================
    private static void AddCpuTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "CPU-Z", Category = "CPU工具",
            Description = "检测 CPU 型号、核心数、缓存、主板、内存等详细硬件信息",
            OfficialUrl = "https://www.cpuid.com/softwares/cpu-z.html",
            WingetId = "CPUID.CPU-Z", RiskLevel = "推荐" },
        new() { Name = "AIDA64 Extreme", Category = "CPU工具",
            Description = "专业系统检测与基准测试，覆盖硬件检测、压力测试、传感器监控",
            OfficialUrl = "https://www.aida64.com/downloads",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "Prime95", Category = "CPU工具",
            Description = "CPU 压力测试与稳定性验证，超频后必测，支持 AVX 指令集",
            OfficialUrl = "https://www.mersenne.org/download/",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "Core Temp", Category = "CPU工具",
            Description = "轻量级 CPU 温度监控，任务栏实时显示每核心温度和负载",
            OfficialUrl = "https://www.alcpu.com/CoreTemp/",
            WingetId = "ALCPU.CoreTemp", RiskLevel = "安全" },
        new() { Name = "ThrottleStop", Category = "CPU工具",
            Description = "Intel 处理器功耗与降压调节，有效降低温度，解除功耗墙限制",
            OfficialUrl = "https://www.techpowerup.com/download/techpowerup-throttlestop/",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "LinX", Category = "CPU工具",
            Description = "Intel Linpack 前端，CPU 极限压力测试，比 Prime95 更快测出不稳定",
            OfficialUrl = "https://github.com/Mysticial/NumberFactory",
            GithubUrl = "https://github.com/Mysticial/NumberFactory",
            WingetId = "", RiskLevel = "注意" }
    });

    // ============================================================
    // 3. 显卡工具 (6)
    // ============================================================
    private static void AddGpuTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "GPU-Z", Category = "显卡工具",
            Description = "检测显卡型号、显存、频率、温度、BIOS 版本等详细信息",
            OfficialUrl = "https://www.techpowerup.com/gpuz/",
            WingetId = "TechPowerUp.GPU-Z", RiskLevel = "推荐" },
        new() { Name = "FurMark", Category = "显卡工具",
            Description = "显卡压力测试与温度检测，俗称甜甜圈，验证散热与稳定性",
            OfficialUrl = "https://geeks3d.com/furmark/",
            WingetId = "Geeks3D.FurMark", RiskLevel = "注意" },
        new() { Name = "MSI Afterburner", Category = "显卡工具",
            Description = "显卡超频工具，支持频率/电压/风扇曲线调节与 OSD 监控",
            OfficialUrl = "https://www.msi.com/Landing/afterburner/graphics-cards",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "NVIDIA Profile Inspector", Category = "显卡工具",
            Description = "N 卡驱动级参数深度调节，修改驱动配置文件，解锁隐藏设置",
            GithubUrl = "https://github.com/Orbmu2k/nvidiaProfileInspector",
            OfficialUrl = "https://github.com/Orbmu2k/nvidiaProfileInspector",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "Display Driver Uninstaller", Category = "显卡工具",
            Description = "DDU 显卡驱动彻底清除工具，安全模式下完全清理驱动残留",
            OfficialUrl = "https://www.guru3d.com/download/display-driver-uninstaller-ddu/",
            WingetId = "Wagnardsoft.DisplayDriverUninstaller", RiskLevel = "注意" },
        new() { Name = "GpuTest", Category = "显卡工具",
            Description = "跨平台 GPU 压力测试，含 FurMark/TessMark/Pixmark 多种场景",
            OfficialUrl = "https://www.geeks3d.com/gputest/",
            WingetId = "", RiskLevel = "注意" }
    });

    // ============================================================
    // 4. 内存工具 (4)
    // ============================================================
    private static void AddMemoryTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "MemTest86", Category = "内存工具",
            Description = "专业内存检测工具，通过 U 盘启动深度测试内存稳定性",
            OfficialUrl = "https://www.memtest86.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "TestMem5", Category = "内存工具",
            Description = "DDR4/DDR5 内存稳定性测试，支持 anta777/extreme 等配置方案",
            GithubUrl = "https://github.com/integralfx/MemTestHelper",
            OfficialUrl = "https://github.com/integralfx/MemTestHelper",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "Thaiphoon Burner", Category = "内存工具",
            Description = "内存 SPD 信息读取，查看颗粒厂商/型号/时序/XMP 等详细信息",
            OfficialUrl = "https://www.thaiphoonburner.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "ZenTimings", Category = "内存工具",
            Description = "AMD Ryzen 内存时序实时查看，一键显示全部小参和 Infinity Fabric",
            GithubUrl = "https://github.com/irusanov/ZenTimings",
            OfficialUrl = "https://github.com/irusanov/ZenTimings",
            WingetId = "", RiskLevel = "安全" }
    });

    // ============================================================
    // 5. 磁盘工具 (8)
    // ============================================================
    private static void AddDiskTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "CrystalDiskInfo", Category = "磁盘工具",
            Description = "硬盘 S.M.A.R.T. 健康状态检测，温度/通电时间/读写量一目了然",
            OfficialUrl = "https://crystalmark.info/",
            WingetId = "CrystalDewWorld.CrystalDiskInfo", RiskLevel = "推荐" },
        new() { Name = "CrystalDiskMark", Category = "磁盘工具",
            Description = "磁盘读写速度基准测试，支持顺序/随机多种 IO 模式测试",
            OfficialUrl = "https://crystalmark.info/",
            WingetId = "CrystalDewWorld.CrystalDiskMark", RiskLevel = "安全" },
        new() { Name = "DiskGenius", Category = "磁盘工具",
            Description = "磁盘分区管理、数据恢复、备份克隆，国产专业磁盘工具",
            OfficialUrl = "https://www.diskgenius.cn/",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "AS SSD Benchmark", Category = "磁盘工具",
            Description = "SSD 性能基准测试，检测 4K 对齐、AHCI 模式与 IOPS 表现",
            OfficialUrl = "https://www.alex-is.de/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "HDTune", Category = "磁盘工具",
            Description = "经典硬盘基准测试，支持读写速度曲线/坏道扫描/健康状态检测",
            OfficialUrl = "https://www.hdtune.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "ATTO Disk Benchmark", Category = "磁盘工具",
            Description = "专业磁盘 I/O 基准测试，多块大小测试 IO 吞吐，企业级存储常用",
            OfficialUrl = "https://www.atto.com/disk-benchmark/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "TxBENCH", Category = "磁盘工具",
            Description = "SSD 基准测试与安全擦除，支持 NVMe 驱动级测试，日本老牌工具",
            OfficialUrl = "https://www.texim.jp/txbenchus.html",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "h2testw", Category = "磁盘工具",
            Description = "U 盘/MicroSD 真容量检测，写入验证，识别扩容假卡/假 U 盘",
            OfficialUrl = "https://www.heise.de/download/product/h2testw-50539",
            WingetId = "", RiskLevel = "安全" }
    });

    // ============================================================
    // 6. 屏幕工具 (3)
    // ============================================================
    private static void AddScreenTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "坏点检测工具", Category = "屏幕工具",
            Description = "屏幕坏点与漏光检测，支持纯色/渐变测试，快速判断屏幕品质",
            OfficialUrl = "https://www.eizo.be/monitor-test/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "色域检测工具", Category = "屏幕工具",
            Description = "检测显示器色域覆盖(sRGB/Adobe RGB/DCI-P3)，在线或本地测试",
            OfficialUrl = "https://www.displaycal.net/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "Color Sustainer", Category = "屏幕工具",
            Description = "校色 ICC 文件加载保持工具，防止系统重置校色配置文件",
            GithubUrl = "https://github.com/M2Team/ColorSustainer",
            OfficialUrl = "https://github.com/M2Team/ColorSustainer",
            WingetId = "", RiskLevel = "安全" }
    });

    // ============================================================
    // 7. 综合检测 (5)
    // ============================================================
    private static void AddComprehensiveTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "HWiNFO", Category = "综合检测",
            Description = "专业级系统信息与硬件监控，传感器数据最全面，实时监控",
            OfficialUrl = "https://www.hwinfo.com/",
            WingetId = "MartinHWiNFO.HWiNFO", RiskLevel = "推荐" },
        new() { Name = "BatteryInfoView", Category = "综合检测",
            Description = "NirSoft 电池信息查看，设计容量/实际容量/损耗率/循环次数",
            OfficialUrl = "https://www.nirsoft.net/utils/battery_information_view.html",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "SpeedFan", Category = "综合检测",
            Description = "硬件温度与风扇转速监控，支持自定义风扇曲线和自动调速",
            OfficialUrl = "https://www.almico.com/speedfan.php",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "Speccy", Category = "综合检测",
            Description = "Piriform 出品系统信息工具，一目了然的硬件概览和温度报告",
            OfficialUrl = "https://www.ccleaner.com/speccy",
            WingetId = "Piriform.Speccy", RiskLevel = "安全" },
        new() { Name = "HWMonitor", Category = "综合检测",
            Description = "全硬件传感器监控，实时查看 CPU/GPU/主板/硬盘温度电压",
            OfficialUrl = "https://www.cpuid.com/softwares/hwmonitor.html",
            WingetId = "CPUID.HWMonitor", RiskLevel = "安全" }
    });

    // ============================================================
    // 8. 网络工具 (3)
    // ============================================================
    private static void AddNetworkTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "网络连接", Category = "网络工具",
            Description = "管理网络适配器和连接设置，快速查看 IP/DNS/适配器状态",
            LaunchPath = "ncpa.cpl", RiskLevel = "安全" },
        new() { Name = "Speedtest", Category = "网络工具",
            Description = "全球最流行的网络测速工具，测试下载/上传带宽和延迟",
            OfficialUrl = "https://www.speedtest.net/",
            WingetId = "Ookla.Speedtest.Desktop", RiskLevel = "安全" },
        new() { Name = "资源监视器", Category = "网络工具",
            Description = "查看 CPU/内存/磁盘/网络 的详细使用情况，定位资源占用进程",
            LaunchPath = "resmon.exe", RiskLevel = "安全" }
    });

    // ============================================================
    // 9. 外设工具 (4)
    // ============================================================
    private static void AddPeripheralTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Keyboard Test Utility", Category = "外设工具",
            Description = "键盘按键测试，检测按键冲突/连键/无响应，支持多键同按测试",
            OfficialUrl = "https://www.keyboardtestutility.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "MOUSERATE", Category = "外设工具",
            Description = "鼠标回报率测试，检测 USB 轮询率/双击/丢帧问题",
            GithubUrl = "https://github.com/minetest-j55/MouseTester",
            OfficialUrl = "https://github.com/minetest-j55/MouseTester",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "MouseTester", Category = "外设工具",
            Description = "鼠标性能分析，绘制移动轨迹/加速度/平滑度图表",
            GithubUrl = "https://github.com/octeep/MouseTester",
            OfficialUrl = "https://github.com/octeep/MouseTester",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "KeyTweak", Category = "外设工具",
            Description = "Windows 键盘键位映射修改，无需驱动即可重构键盘布局",
            OfficialUrl = "https://www.keytweak.com/",
            WingetId = "", RiskLevel = "注意" }
    });

    // ============================================================
    // 10. 游戏工具 (4)
    // ============================================================
    private static void AddGameTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "DXVAChecker", Category = "游戏工具",
            Description = "DirectX 视频加速检查，查看编解码器/GPU 加速/DXVA 硬件解码支持",
            GithubUrl = "https://github.com/Starboihub/DXVAChecker",
            OfficialUrl = "https://github.com/Starboihub/DXVAChecker",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "游戏加速", Category = "游戏工具",
            Description = "一键切换高性能电源计划 + 关闭 Defender 实时防护，游戏结束后恢复",
            LaunchPath = "scenario:game-accelerate", RiskLevel = "注意" },
        new() { Name = "显示帧率", Category = "游戏工具",
            Description = "启动 RivaTuner Statistics Server (RTSS) 屏幕帧率显示，需先安装 MSI Afterburner",
            LaunchPath = "rtss.exe", OfficialUrl = "https://www.msi.com/Landing/afterburner/graphics-cards",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "游戏优化", Category = "游戏工具",
            Description = "打开 Windows 游戏模式设置，启用 GPU 硬件加速和可变刷新率",
            LaunchPath = "ms-settings:gaming-gamemode", RiskLevel = "安全" }
    });

    // ============================================================
    // 11. 其他工具 (5)
    // ============================================================
    private static void AddOtherTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Ventoy", Category = "其他工具",
            Description = "多合一 U 盘启动盘制作，ISO/WIM/IMG/VHD 直接复制即启动，无需反复格式化",
            OfficialUrl = "https://www.ventoy.net/",
            WingetId = "Ventoy.Ventoy", RiskLevel = "推荐" },
        new() { Name = "Rufus", Category = "其他工具",
            Description = "USB 启动盘制作工具，支持 ISO/DD 模式写入，UEFI/BIOS 双兼容",
            OfficialUrl = "https://rufus.ie/",
            WingetId = "Rufus.Rufus", RiskLevel = "推荐" },
        new() { Name = "UltraISO", Category = "其他工具",
            Description = "光盘映像编辑/转换/刻录，制作/编辑 ISO 文件，U 盘启动盘写入",
            OfficialUrl = "https://www.ultraiso.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "Snipaste", Category = "其他工具",
            Description = "截图 + 贴图工具，支持像素级截图取色和屏幕贴图，设计师效率利器",
            OfficialUrl = "https://www.snipaste.com/",
            WingetId = "Snipaste.Snipaste", RiskLevel = "推荐" },
        new() { Name = "ScreenToGif", Category = "其他工具",
            Description = "屏幕录制为 GIF 动画，支持编辑器裁剪、帧率调节、文字标注",
            OfficialUrl = "https://www.screentogif.com/",
            WingetId = "NickeManarin.ScreenToGif", RiskLevel = "推荐" }
    });

    // ============================================================
    // 12. 品牌工具 (11)
    // ============================================================
    private static void AddBrandTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Lenovo Legion Toolkit", Category = "品牌工具",
            Description = "替代官方Legion Zone/联想管家，性能模式切换、风扇曲线、独显热切换、电池阈值、键盘背光",
            GithubUrl = "https://github.com/BartoszCichecki/LenovoLegionToolkit/releases",
            WingetId = "BartoszCichecki.LenovoLegionToolkit", RiskLevel = "推荐" },
        new() { Name = "G-Helper", Category = "品牌工具",
            Description = "奥创中心完美平替，单文件绿色。性能模式、风扇曲线、独显直连、屏幕OD、键盘RGB、电池充电上限控制",
            OfficialUrl = "https://g-helper.com",
            GithubUrl = "https://github.com/seerge/g-helper", RiskLevel = "推荐" },
        new() { Name = "Armoury Crate 奥创中心", Category = "品牌工具",
            Description = "华硕官方系统控制套件，支持ROG/天选/无畏/灵耀全系列",
            OfficialUrl = "https://www.asus.com.cn/support/", RiskLevel = "安全" },
        new() { Name = "OmenSuperHub", Category = "品牌工具",
            Description = "替代官方OMEN Gaming Hub，完全离线无广告。WMI BIOS直控风扇、解除DB功耗墙、性能模式切换",
            GithubUrl = "https://github.com/", RiskLevel = "注意" },
        new() { Name = "OMEN Gaming Hub", Category = "品牌工具",
            Description = "HP官方游戏控制中心，灯光、性能、网络优化一站式管理",
            OfficialUrl = "https://www.microsoft.com/store/", RiskLevel = "安全" },
        new() { Name = "TCC-G15", Category = "品牌工具",
            Description = "G模式一键切换+风扇调节，可与官方AWCC共存，支持G15/G16及部分外星人",
            GithubUrl = "https://github.com/", RiskLevel = "注意" },
        new() { Name = "Alienware Command Center", Category = "品牌工具",
            Description = "戴尔/Alienware官方系统控制中心，灯光、超频、散热全控制",
            OfficialUrl = "https://www.dell.com/support/", RiskLevel = "安全" },
        new() { Name = "Fan Control", Category = "品牌工具",
            Description = "全品牌兼容风扇曲线自定义、多温度源联动，无后台常驻，替代MSI Center臃肿套件",
            GithubUrl = "https://github.com/Rem0o/FanControl.Releases",
            WingetId = "Rem0o.FanControl", RiskLevel = "推荐" },
        new() { Name = "MSI Center", Category = "品牌工具",
            Description = "微星官方系统控制与性能管理套件",
            OfficialUrl = "https://www.msi.com/support/", RiskLevel = "安全" },
        new() { Name = "OpenRGB", Category = "品牌工具",
            Description = "全品牌RGB灯光统一控制，替代各厂原厂灯效软件，支持绝大部分RGB设备",
            GithubUrl = "https://gitlab.com/CalcProgrammer1/OpenRGB",
            WingetId = "CalcProgrammer1.OpenRGB", RiskLevel = "安全" },
        new() { Name = "Gaming Center (修改版)", Category = "品牌工具",
            Description = "机械革命解锁官方阉割的功耗调节功能，自定义显卡功耗上限、SPC性能设置、风扇曲线",
            RiskLevel = "注意" }
    });
}
