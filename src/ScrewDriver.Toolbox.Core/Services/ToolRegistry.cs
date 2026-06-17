using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Core.Services;

public static class ToolRegistry
{
    private static List<ToolItem>? _allTools;
    private static List<ToolItem>? _customTools;
    private static readonly JsonConfigManager _config = new(AppDomain.CurrentDomain.BaseDirectory);

    public static List<string> Categories { get; } = new()
    {
        "系统工具", "CPU工具", "主板工具", "内存工具",
        "显卡工具", "硬盘工具", "屏幕工具", "外设工具",
        "安全工具", "联想工具", "华硕工具", "惠普工具", "戴尔工具", "微星工具", "机械革命", "通用工具", "启动与镜像",
        "游戏工具", "烤鸡工具", "音视频播放器", "音视频处理工具", "图像与设计工具", "综合检测", "其他工具"
    };

    public static List<ToolItem> GetAllTools()
    {
        if (_allTools == null)
        {
            _allTools = new List<ToolItem>();
            AddSystemTools();
            AddSecurityTools();
            AddNetworkTools();
            AddHardwareDetection();
            AddBenchmarkTools();
            AddPeripheralScreen();
            AddBrandTools();
            AddOtherTools();
            AddGameTools();
            AddBenchTools();
            AddMediaTools();
            AddMediaProcessTools();
            AddDesignTools();
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
    // 1. 系统工具 (15)
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
        new() { Name = "磁盘分析", Category = "硬盘工具",
            Description = "极速磁盘空间分析，MFT 直读秒出结果，直观定位大文件",
            OfficialUrl = "https://diskanalyzer.com/",
            WingetId = "AntibodySoftware.WizTree", RiskLevel = "推荐" },
        new() { Name = "DirectX 修复工具", Category = "系统工具",
            Description = "DirectX 组件修复与运行库补全，一键检测并修复缺失 DLL",
            OfficialUrl = "https://www.crsky.com/soft/77560.html",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "蓝屏分析", Category = "系统工具",
            Description = "NirSoft 蓝屏转储分析，自动解析 minidump 文件定位崩溃驱动",
            OfficialUrl = "https://www.nirsoft.net/utils/blue_screen_view.html",
            WingetId = "NirSoft.BlueScreenView", RiskLevel = "安全" },
        new() { Name = "DiskGenius", Category = "硬盘工具",
            Description = "磁盘分区管理、数据恢复、备份克隆，国产专业磁盘工具",
            OfficialUrl = "https://www.diskgenius.cn/",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "右键菜单管理", Category = "系统工具",
            Description = "管理 Windows 右键菜单和 Shell 扩展，禁用卡顿来源的第三方扩展",
            LaunchPath = "ms-settings:installed-apps", RiskLevel = "注意" },
        new() { Name = "SuperPI", Category = "CPU工具",
            Description = "经典单核性能测试工具，通过计算圆周率评估CPU单线程性能",
            OfficialUrl = "https://www.techpowerup.com/download/superpi/", RiskLevel = "安全" },
        new() { Name = "Win11 轻松设置", Category = "系统工具",
            Description = "Windows 11 系统设置优化工具，一键关闭广告/禁用Defender/调整隐私/恢复经典菜单等",
            OfficialUrl = "https://www.bilibili.com/opus/904672369138729017/",
            GithubUrl = "https://github.com/btwise/Win11-Easy-Set", RiskLevel = "注意" },
        new() { Name = "LocalSend", Category = "系统工具",
            Description = "开源跨平台局域网文件传输工具，无需互联网",
            GithubUrl = "https://github.com/localsend/localsend",
            OfficialUrl = "https://localsend.org/",
            WingetId = "LocalSend.LocalSend", RiskLevel = "推荐" },
        new() { Name = "Flix", Category = "系统工具",
            Description = "跨平台局域网文件传输工具，无需网络即可高速传输",
            GithubUrl = "https://github.com/pppscn/Flix",
            OfficialUrl = "https://flixcn.com/", RiskLevel = "安全" },
        new() { Name = "Quicker", Category = "系统工具",
            Description = "鼠标指指即点的效率工具，快速启动面板、手势操作、文本面板",
            OfficialUrl = "https://getquicker.net/",
            WingetId = "LiErHeXun.Quicker", RiskLevel = "安全" },
        new() { Name = "QuickLook", Category = "系统工具",
            Description = "敲空格快速预览文件，支持图片/文档/视频/压缩包等格式",
            GithubUrl = "https://github.com/QL-Win/QuickLook",
            OfficialUrl = "https://github.com/QL-Win/QuickLook",
            WingetId = "QL-Win.QuickLook", RiskLevel = "安全" },
        new() { Name = "MPV", Category = "系统工具",
            Description = "轻量开源视频播放器，GPU 渲染、低延迟、高自定义",
            OfficialUrl = "https://mpv.io/",
            WingetId = "shinchiro.mpv", RiskLevel = "安全" },
        new() { Name = "Bandizip", Category = "系统工具",
            Description = "轻量快速的文件压缩/解压工具，支持多种格式，界面简洁",
            OfficialUrl = "https://www.bandisoft.com/bandizip/",
            WingetId = "Bandisoft.Bandizip", RiskLevel = "推荐" },
        new() { Name = "HiBit Uninstaller", Category = "系统工具",
            Description = "强大的软件卸载工具，支持强制卸载、残留清理、绿色软件扫描",
            OfficialUrl = "https://www.hibitsoft.ir/HiBitUninstaller/",
            WingetId = "HiBitSoftware.HiBitUninstaller", RiskLevel = "安全" },
    });

    // ============================================================
    // 2. 安全与优化 (9)
    // ============================================================
    private static void AddSecurityTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Defender 控制", Category = "安全工具",
            Description = "打开 Windows 安全中心，管理病毒防护、防火墙和账户保护设置",
            LaunchPath = "windowsdefender://", RiskLevel = "安全" },
        new() { Name = "KMS 激活", Category = "其他工具",
            Description = "Windows/Office 批量激活管理，查看激活状态、设置 KMS 服务器、产品密钥管理",
            LaunchPath = "cmd.exe /k slmgr.vbs -dlv", RiskLevel = "注意" },
        new() { Name = "游戏优化", Category = "安全工具",
            Description = "打开 Windows 游戏模式设置，启用 GPU 硬件加速和可变刷新率",
            LaunchPath = "ms-settings:gaming-gamemode", RiskLevel = "安全" },
        new() { Name = "DDU", Category = "显卡工具",
            Description = "显卡驱动彻底清除工具，安全模式下完全清理驱动残留",
            OfficialUrl = "https://www.guru3d.com/download/display-driver-uninstaller-ddu/",
            WingetId = "Wagnardsoft.DisplayDriverUninstaller", RiskLevel = "注意" },
        new() { Name = "MSI Afterburner", Category = "显卡工具",
            Description = "显卡超频工具，支持频率/电压/风扇曲线调节与 OSD 监控",
            OfficialUrl = "https://www.msi.com/Landing/afterburner/graphics-cards",
            WingetId = "Guru3D.MSIAfterburner", RiskLevel = "注意" },
        new() { Name = "NVIDIA Profile Inspector", Category = "显卡工具",
            Description = "N 卡驱动级参数深度调节，修改驱动配置文件，解锁隐藏设置",
            GithubUrl = "https://github.com/Orbmu2k/nvidiaProfileInspector",
            OfficialUrl = "https://github.com/Orbmu2k/nvidiaProfileInspector",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "ThrottleStop", Category = "CPU工具",
            Description = "Intel 处理器功耗与降压调节，有效降低温度，解除功耗墙限制",
            OfficialUrl = "https://www.techpowerup.com/download/techpowerup-throttlestop/",
            WingetId = "", RiskLevel = "注意" }
    });

    // ============================================================
    // 3. 网络工具 (5)
    // ============================================================
    private static void AddNetworkTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "网速测试", Category = "其他工具",
            Description = "全球最流行的网络测速工具，测试下载/上传带宽和延迟",
            OfficialUrl = "https://www.speedtest.net/",
            WingetId = "Ookla.Speedtest.Desktop", RiskLevel = "安全" },
        new() { Name = "网络连接", Category = "其他工具",
            Description = "管理网络适配器和连接设置，快速查看 IP/DNS/适配器状态",
            LaunchPath = "ncpa.cpl", RiskLevel = "安全" },
        new() { Name = "端口占用", Category = "其他工具",
            Description = "查看本机端口占用情况，定位占用进程，排查端口冲突问题",
            LaunchPath = "resmon.exe", RiskLevel = "安全" },
        new() { Name = "Hosts 编辑", Category = "其他工具",
            Description = "以管理员权限打开 hosts 文件，自定义域名到 IP 的本地解析映射",
            LaunchPath = @"notepad C:\Windows\System32\drivers\etc\hosts", RiskLevel = "注意" },
        new() { Name = "WiFi 密码", Category = "其他工具",
            Description = "查看当前连接 WiFi 的密码，显示已保存的所有无线网络配置文件",
            LaunchPath = "cmd.exe /k netsh wlan show profiles", RiskLevel = "安全" }
    });

    // ============================================================
    // 4. 硬件检测 (12)
    // ============================================================
    private static void AddHardwareDetection() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "CPU-Z", Category = "CPU工具",
            Description = "检测 CPU 型号、核心数、缓存、主板、内存等详细硬件信息",
            OfficialUrl = "https://www.cpuid.com/softwares/cpu-z.html",
            WingetId = "CPUID.CPU-Z", RiskLevel = "推荐" },
        new() { Name = "GPU-Z", Category = "显卡工具",
            Description = "检测显卡型号、显存、频率、温度、BIOS 版本等详细信息",
            OfficialUrl = "https://www.techpowerup.com/gpuz/",
            WingetId = "TechPowerUp.GPU-Z", RiskLevel = "推荐" },
        new() { Name = "硬件监控", Category = "主板工具",
            Description = "专业级系统信息与硬件监控，传感器数据最全面，实时监控温度/电压/频率",
            OfficialUrl = "https://www.hwinfo.com/",
            WingetId = "MartinHWiNFO.HWiNFO", RiskLevel = "推荐" },
        new() { Name = "AIDA64 Extreme", Category = "主板工具",
            Description = "专业系统检测与基准测试，覆盖硬件检测、压力测试、传感器监控",
            OfficialUrl = "https://www.aida64.com/downloads",
            WingetId = "FinalWire.AIDA64", RiskLevel = "安全" },
        new() { Name = "Core Temp", Category = "CPU工具",
            Description = "轻量级 CPU 温度监控，任务栏实时显示每核心温度和负载",
            OfficialUrl = "https://www.alcpu.com/CoreTemp/",
            WingetId = "ALCPU.CoreTemp", RiskLevel = "安全" },
        new() { Name = "CrystalDiskInfo", Category = "硬盘工具",
            Description = "硬盘 S.M.A.R.T. 健康状态检测，温度/通电时间/读写量一目了然",
            OfficialUrl = "https://crystalmark.info/",
            WingetId = "CrystalDewWorld.CrystalDiskInfo", RiskLevel = "推荐" },
        new() { Name = "HWMonitor", Category = "主板工具",
            Description = "全硬件传感器监控，实时查看 CPU/GPU/主板/硬盘温度电压",
            OfficialUrl = "https://www.cpuid.com/softwares/hwmonitor.html",
            WingetId = "CPUID.HWMonitor", RiskLevel = "安全" },
        new() { Name = "Speccy", Category = "主板工具",
            Description = "Piriform 出品系统信息工具，一目了然的硬件概览和温度报告",
            OfficialUrl = "https://www.ccleaner.com/speccy",
            WingetId = "Piriform.Speccy", RiskLevel = "安全" },
        new() { Name = "Thaiphoon Burner", Category = "内存工具",
            Description = "内存 SPD 信息读取，查看颗粒厂商/型号/时序/XMP 等详细信息",
            OfficialUrl = "https://www.thaiphoonburner.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "MemTest64", Category = "内存工具",
            Description = "64位系统内存稳定性测试，支持多线程并行测试",
            OfficialUrl = "https://www.techpowerup.com/download/techpowerup-memtest64/", RiskLevel = "安全" },
        new() { Name = "ZenTimings", Category = "内存工具",
            Description = "AMD Ryzen 内存时序实时查看，一键显示全部小参和 Infinity Fabric",
            GithubUrl = "https://github.com/irusanov/ZenTimings",
            OfficialUrl = "https://github.com/irusanov/ZenTimings",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "BatteryInfoView", Category = "其他工具",
            Description = "NirSoft 电池信息查看，设计容量/实际容量/损耗率/循环次数",
            OfficialUrl = "https://www.nirsoft.net/utils/battery_information_view.html",
            WingetId = "NirSoft.BatteryInfoView", RiskLevel = "安全" },
        new() { Name = "SpeedFan", Category = "主板工具",
            Description = "硬件温度与风扇转速监控，支持自定义风扇曲线和自动调速",
            OfficialUrl = "https://www.almico.com/speedfan.php",
            WingetId = "AlfredoComparetti.SpeedFan", RiskLevel = "注意" }
    });

    // ============================================================
    // 5. 性能测试 (13)
    // ============================================================
    private static void AddBenchmarkTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "磁盘测试", Category = "硬盘工具",
            Description = "硬盘读写速度基准测试，支持顺序/随机多种 IO 模式测试",
            OfficialUrl = "https://crystalmark.info/",
            WingetId = "CrystalDewWorld.CrystalDiskMark", RiskLevel = "安全" },
        new() { Name = "AS SSD Benchmark", Category = "硬盘工具",
            Description = "SSD 性能基准测试，检测 4K 对齐、AHCI 模式与 IOPS 表现",
            OfficialUrl = "https://www.alex-is.de/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "HDTune", Category = "硬盘工具",
            Description = "经典硬盘基准测试，支持读写速度曲线/坏道扫描/健康状态检测",
            OfficialUrl = "https://www.hdtune.com/",
            WingetId = "HDTune.HDTune", RiskLevel = "安全" },
        new() { Name = "ATTO Disk Benchmark", Category = "硬盘工具",
            Description = "专业磁盘 I/O 基准测试，多块大小测试 IO 吞吐，企业级存储常用",
            OfficialUrl = "https://www.atto.com/disk-benchmark/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "TxBENCH", Category = "硬盘工具",
            Description = "SSD 基准测试与安全擦除，支持 NVMe 驱动级测试，日本老牌工具",
            OfficialUrl = "https://www.texim.jp/txbenchus.html",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "傲梅分区助手", Category = "硬盘工具",
            Description = "免费的磁盘分区管理工具，支持扩大/合并/拆分分区",
            OfficialUrl = "https://www.disktool.cn/",
            WingetId = "AOMEI.PartitionAssistant", RiskLevel = "注意" },
        new() { Name = "SpaceSniffer", Category = "硬盘工具",
            Description = "可视化磁盘空间分析，区块图展示文件和文件夹大小",
            OfficialUrl = "https://www.uderzo.it/main_products/space_sniffer/",
            WingetId = "Uderzo.SpaceSniffer", RiskLevel = "安全" },
        new() { Name = "Defraggler", Category = "硬盘工具",
            Description = "磁盘碎片整理工具，支持单个文件或整个磁盘快速整理",
            OfficialUrl = "https://www.ccleaner.com/defraggler",
            WingetId = "Piriform.Defraggler", RiskLevel = "安全" },
        new() { Name = "h2testw", Category = "硬盘工具",
            Description = "U 盘/MicroSD 真容量检测，写入验证，识别扩容假卡/假 U 盘",
            OfficialUrl = "https://www.heise.de/download/product/h2testw-50539",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "GpuTest", Category = "显卡工具",
            Description = "跨平台 GPU 压力测试，含 FurMark/TessMark/Pixmark 多种场景",
            OfficialUrl = "https://www.geeks3d.com/gputest/",
            WingetId = "Geeks3D.GpuTest", RiskLevel = "注意" },
        
                new() { Name = "MemTest86", Category = "内存工具",
            Description = "专业内存检测工具，通过 U 盘启动深度测试内存稳定性",
            OfficialUrl = "https://www.memtest86.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "TestMem5", Category = "内存工具",
            Description = "DDR4/DDR5 内存稳定性测试，支持 anta777/extreme 等配置方案",
            GithubUrl = "https://github.com/integralfx/MemTestHelper",
            OfficialUrl = "https://github.com/integralfx/MemTestHelper",
            WingetId = "", RiskLevel = "注意" },
        new() { Name = "电池消耗分析", Category = "其他工具",
            Description = "生成 Windows 电池使用详细报告，分析应用耗电和电池健康状态",
            LaunchPath = "cmd.exe /c powercfg /batteryreport /output %temp%\\battery-report.html && start %temp%\\battery-report.html",
            RiskLevel = "安全" }
    });

    // ============================================================
    // 6. 外设与屏幕 (8)
    // ============================================================
    private static void AddPeripheralScreen() => _allTools!.AddRange(new List<ToolItem>
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
            WingetId = "", RiskLevel = "安全" },
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
        new() { Name = "AresonMouseTest", Category = "外设工具",
            Description = "鼠标按键测试工具，检测鼠标按键回弹、双击和延迟问题",
            OfficialUrl = "http://www.softpedia.com/get/System/System-Miscellaneous/Areson-Mouse-Test.shtml", RiskLevel = "安全" },
        new() { Name = "KeyTweak", Category = "外设工具",
            Description = "Windows 键盘键位映射修改，无需驱动即可重构键盘布局",
            OfficialUrl = "https://www.keytweak.com/",
            WingetId = "", RiskLevel = "注意" }
    });

    // ============================================================
    // 7. 品牌工具 (11)
    // ============================================================
    private static void AddBrandTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Lenovo Legion Toolkit", Category = "联想工具",
            Description = "替代官方Legion Zone/联想管家，性能模式切换、风扇曲线、独显热切换、电池阈值、键盘背光",
            GithubUrl = "https://github.com/BartoszCichecki/LenovoLegionToolkit/releases",
            WingetId = "BartoszCichecki.LenovoLegionToolkit", RiskLevel = "推荐" },
        new() { Name = "G-Helper", Category = "华硕工具",
            Description = "奥创中心完美平替，单文件绿色。性能模式、风扇曲线、独显直连、屏幕OD、键盘RGB、电池充电上限控制",
            OfficialUrl = "https://g-helper.com",
            GithubUrl = "https://github.com/seerge/g-helper", RiskLevel = "推荐" },
        new() { Name = "Armoury Crate 奥创中心", Category = "华硕工具",
            Description = "华硕官方系统控制套件，支持ROG/天选/无畏/灵耀全系列",
            OfficialUrl = "https://www.asus.com.cn/support/", RiskLevel = "安全" },
        new() { Name = "OmenSuperHub", Category = "惠普工具",
            Description = "替代官方OMEN Gaming Hub，完全离线无广告。WMI BIOS直控风扇、解除DB功耗墙、性能模式切换",
            GithubUrl = "https://github.com/nn9dev/OmenSuperHub",
            OfficialUrl = "https://github.com/nn9dev/OmenSuperHub", RiskLevel = "注意" },
        new() { Name = "OMEN Gaming Hub", Category = "惠普工具",
            Description = "HP官方游戏控制中心，灯光、性能、网络优化一站式管理",
            OfficialUrl = "https://www.microsoft.com/store/", RiskLevel = "安全" },
        new() { Name = "TCC-G15", Category = "戴尔工具",
            Description = "G模式一键切换+风扇调节，可与官方AWCC共存，支持G15/G16及部分外星人",
            GithubUrl = "https://github.com/JT1CD/TCC-G15",
            OfficialUrl = "https://github.com/JT1CD/TCC-G15", RiskLevel = "注意" },
        new() { Name = "Alienware Command Center", Category = "戴尔工具",
            Description = "戴尔/Alienware官方系统控制中心，灯光、超频、散热全控制",
            OfficialUrl = "https://www.dell.com/support/", RiskLevel = "安全" },
        new() { Name = "Fan Control", Category = "通用工具",
            Description = "全品牌兼容风扇曲线自定义、多温度源联动，无后台常驻，替代MSI Center臃肿套件",
            GithubUrl = "https://github.com/Rem0o/FanControl.Releases",
            WingetId = "Rem0o.FanControl", RiskLevel = "推荐" },
        new() { Name = "MSI Center", Category = "微星工具",
            Description = "微星官方系统控制与性能管理套件",
            OfficialUrl = "https://www.msi.com/support/", RiskLevel = "安全" },
        new() { Name = "OpenRGB", Category = "通用工具",
            Description = "全品牌RGB灯光统一控制，替代各厂原厂灯效软件，支持绝大部分RGB设备",
            GithubUrl = "https://gitlab.com/CalcProgrammer1/OpenRGB",
            WingetId = "CalcProgrammer1.OpenRGB", RiskLevel = "安全" },
        new() { Name = "Gaming Center (修改版)", Category = "机械革命",
            Description = "机械革命解锁官方阉割的功耗调节功能，自定义显卡功耗上限、SPC性能设置、风扇曲线",
            RiskLevel = "注意" }
    });

    // ============================================================
    // 8. 其他工具 (6)
    // ============================================================
    private static void AddOtherTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Ventoy", Category = "启动与镜像",
            Description = "多合一 U 盘启动盘制作，ISO/WIM/IMG/VHD 直接复制即启动，无需反复格式化",
            OfficialUrl = "https://www.ventoy.net/",
            WingetId = "Ventoy.Ventoy", RiskLevel = "推荐" },
        new() { Name = "Rufus", Category = "启动与镜像",
            Description = "USB 启动盘制作工具，支持 ISO/DD 模式写入，UEFI/BIOS 双兼容",
            OfficialUrl = "https://rufus.ie/",
            WingetId = "Rufus.Rufus", RiskLevel = "推荐" },
        new() { Name = "UltraISO", Category = "启动与镜像",
            Description = "光盘映像编辑/转换/刻录，制作/编辑 ISO 文件，U 盘启动盘写入",
            OfficialUrl = "https://www.ultraiso.com/",
            WingetId = "", RiskLevel = "安全" },
        new() { Name = "PixPin", Category = "其他工具",
            Description = "截图/贴图/OCR/长截图/标记，国产效率工具",
            OfficialUrl = "https://pixpin.cn/",
            WingetId = "PixPin.PixPin", RiskLevel = "推荐" },
        new() { Name = "Snipaste", Category = "其他工具",
            Description = "截图 + 贴图工具，支持像素级截图取色和屏幕贴图，设计师效率利器",
            OfficialUrl = "https://www.snipaste.com/",
            WingetId = "Snipaste.Snipaste", RiskLevel = "推荐" },
        new() { Name = "Recuva", Category = "其他工具",
            Description = "免费文件恢复工具，恢复误删除的照片、文档、视频等",
            OfficialUrl = "https://www.ccleaner.com/recuva",
            WingetId = "Piriform.Recuva", RiskLevel = "安全" },
        new() { Name = "ScreenToGif", Category = "其他工具",
            Description = "屏幕录制为 GIF 动画，支持编辑器裁剪、帧率调节、文字标注",
            OfficialUrl = "https://www.screentogif.com/",
            WingetId = "NickeManarin.ScreenToGif", RiskLevel = "推荐" },
        new() { Name = "DXVAChecker", Category = "其他工具",
            Description = "DirectX 视频加速检查，查看编解码器/GPU 加速/DXVA 硬件解码支持",
            OfficialUrl = "https://github.com/Starboihub/DXVAChecker",
            WingetId = "", RiskLevel = "安全" }
    });

    // ============================================================
    // 9. 游戏工具 (4)
    // ============================================================
    private static void AddGameTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Steam", Category = "游戏工具",
            Description = "全球最大的 PC 游戏数字发行平台，数千款游戏一站式管理",
            OfficialUrl = "https://store.steampowered.com/", WingetId = "Valve.Steam", RiskLevel = "安全" },
        new() { Name = "EPIC Games", Category = "游戏工具",
            Description = "EPIC 游戏商店，每周免费游戏、虚幻引擎生态",
            OfficialUrl = "https://store.epicgames.com/", WingetId = "EpicGames.EpicGamesLauncher", RiskLevel = "安全" },
        new() { Name = "EA App", Category = "游戏工具",
            Description = "EA 游戏平台，管理 EA Play 订阅和游戏库",
            OfficialUrl = "https://www.ea.com/ea-app", RiskLevel = "安全" },
        new() { Name = "战网", Category = "游戏工具",
            Description = "暴雪游戏平台，管理魔兽世界、守望先锋、暗黑破坏神等",
            OfficialUrl = "https://www.blizzard.com/apps/battle.net/desktop", RiskLevel = "安全" }
    });

    // ============================================================
    // 10. 烤鸡工具 (4)
    // ============================================================
    private static void AddBenchTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "FurMark", Category = "烤鸡工具",
            Description = "显卡压力测试与温度检测，俗称甜甜圈，验证散热与稳定性",
            OfficialUrl = "https://geeks3d.com/furmark/",
            WingetId = "Geeks3D.FurMark", RiskLevel = "注意" },
        new() { Name = "Prime95", Category = "烤鸡工具",
            Description = "CPU 压力测试与稳定性验证，超频后必测，支持 AVX 指令集",
            OfficialUrl = "https://www.mersenne.org/download/",
            WingetId = "GIMPS.Prime95", RiskLevel = "注意" },
        new() { Name = "LinX", Category = "烤鸡工具",
            Description = "Intel Linpack 前端，CPU 极限压力测试",
            OfficialUrl = "https://www.techpowerup.com/download/linx/",
            GithubUrl = "https://github.com/Mysticial/NumberFactory", RiskLevel = "注意" },
        new() { Name = "CPU Burner", Category = "烤鸡工具",
            Description = "单文件 CPU 满载测试工具，快速检测散热和稳定性",
            OfficialUrl = "https://www.softpedia.com/get/System/Benchmarks/CPU-Burner.shtml", RiskLevel = "注意" }
    });

    // ============================================================
    // 音视频播放器
    // ============================================================
    private static void AddMediaTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "VLC", Category = "音视频播放器",
            Description = "开源跨平台多媒体播放器，支持几乎所有音视频格式",
            OfficialUrl = "https://www.videolan.org/vlc/",
            WingetId = "VideoLAN.VLC", RiskLevel = "推荐" },
        new() { Name = "MPC-BE", Category = "音视频播放器",
            Description = "轻量视频播放器，基于MPC-HC，解码能力强",
            OfficialUrl = "https://mpc-be.org/",
            WingetId = "MPC-BE.MPC-BE", RiskLevel = "推荐" },
        new() { Name = "PotPlayer", Category = "音视频播放器",
            Description = "全能视频播放器，解码能力强、功能丰富、启动快",
            OfficialUrl = "https://potplayer.daum.net/",
            WingetId = "Daum.PotPlayer", RiskLevel = "推荐" }
    });

    // ============================================================
    // 音视频处理工具
    // ============================================================
    private static void AddMediaProcessTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "FFmpeg", Category = "音视频处理工具",
            Description = "开源音视频处理工具，支持格式转换/录制/流处理",
            OfficialUrl = "https://ffmpeg.org/",
            WingetId = "Gyan.FFmpeg", RiskLevel = "安全" },
        new() { Name = "HandBrake", Category = "音视频处理工具",
            Description = "开源视频转码工具，支持批量转换和预设优化",
            OfficialUrl = "https://handbrake.fr/",
            WingetId = "HandBrake.HandBrake", RiskLevel = "推荐" },
        new() { Name = "Mp3tag", Category = "音视频处理工具",
            Description = "音乐文件标签编辑工具，批量修改ID3标签",
            OfficialUrl = "https://www.mp3tag.de/en/",
            WingetId = "Mp3tag.Mp3tag", RiskLevel = "推荐" },
        new() { Name = "File Converter", Category = "音视频处理工具",
            Description = "右键菜单文件格式转换，支持音视频/图片",
            GithubUrl = "https://github.com/Tichau/FileConverter",
            OfficialUrl = "https://file-converter.org/",
            WingetId = "Tichau.FileConverter", RiskLevel = "安全" }
    });

    // ============================================================
    // 图像与设计工具
    // ============================================================
    private static void AddDesignTools() => _allTools!.AddRange(new List<ToolItem>
    {
        new() { Name = "Honeyview", Category = "图像与设计工具",
            Description = "极速图片查看器，支持RAW/PSD/漫画格式",
            OfficialUrl = "https://www.bandisoft.com/honeyview/",
            WingetId = "Bandisoft.Honeyview", RiskLevel = "推荐" },
        new() { Name = "Blender", Category = "图像与设计工具",
            Description = "开源3D建模/动画/渲染/视频编辑全套工具",
            OfficialUrl = "https://www.blender.org/",
            WingetId = "BlenderFoundation.Blender", RiskLevel = "推荐" },
        new() { Name = "Krita", Category = "图像与设计工具",
            Description = "开源数字绘画软件，支持笔刷/图层/动画",
            OfficialUrl = "https://krita.org/",
            WingetId = "KritaFoundation.Krita", RiskLevel = "推荐" }
    });

}
