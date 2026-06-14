namespace ScrewDriver.Toolbox.Services;

public class SystemRepairService : ISystemRepairService
{
    private readonly Dictionary<string, List<RepairSolution>> _scenarios = [];

    public SystemRepairService()
    {
        InitializeScenarios();
    }

    private void InitializeScenarios()
    {
        // 系统卡顿
        _scenarios["system_slow"] =
        [
            new() { Id = "clean_temp", Name = "清理临时文件", Description = "使用系统磁盘清理工具清理临时文件", Command = "cleanmgr /sageset:1", RiskLevel = RiskLevel.Safe },
            new() { Id = "disk_check", Name = "磁盘错误检查", Description = "检查并修复磁盘文件系统错误", Command = "chkdsk /f", RiskLevel = RiskLevel.Low },
            new() { Id = "sfc_scan", Name = "系统文件检查", Description = "扫描并修复损坏的系统文件", Command = "sfc /scannow", RiskLevel = RiskLevel.Safe }
        ];

        // 网络异常
        _scenarios["network_issue"] =
        [
            new() { Id = "dns_flush", Name = "刷新 DNS 缓存", Description = "清除 DNS 解析缓存", Command = "ipconfig /flushdns", RiskLevel = RiskLevel.Safe },
            new() { Id = "winsock_reset", Name = "重置 Winsock", Description = "重置网络协议栈", Command = "netsh winsock reset", RiskLevel = RiskLevel.Low },
            new() { Id = "proxy_check", Name = "检查代理设置", Description = "检查并重置代理配置", Command = "netsh winhttp reset proxy", RiskLevel = RiskLevel.Safe }
        ];

        // 蓝屏分析
        _scenarios["bsod"] =
        [
            new() { Id = "analyze_dump", Name = "分析蓝屏日志", Description = "读取并分析最近的内存转储文件", Command = "dir %SystemRoot%\\Minidump", RiskLevel = RiskLevel.Safe },
            new() { Id = "driver_verify", Name = "驱动程序验证", Description = "检查有问题的驱动程序", Command = "verifier /query", RiskLevel = RiskLevel.Warning }
        ];

        // 更新失败
        _scenarios["update_failed"] =
        [
            new() { Id = "wu_reset", Name = "重置更新组件", Description = "重置 Windows Update 组件", Command = "net stop wuauserv", RiskLevel = RiskLevel.Low },
            new() { Id = "dism_restore", Name = "DISM 恢复", Description = "使用 DISM 修复系统映像", Command = "DISM /Online /Cleanup-Image /RestoreHealth", RiskLevel = RiskLevel.Safe }
        ];

        // 游戏掉帧
        _scenarios["game_lag"] =
        [
            new() { Id = "power_high", Name = "切换高性能电源", Description = "切换到高性能电源计划", Command = "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c", RiskLevel = RiskLevel.Safe },
            new() { Id = "gpu_schedule", Name = "开启 GPU 硬件加速调度", Description = "启用 GPU 硬件加速计划", Command = "", RiskLevel = RiskLevel.Safe }
        ];

        // 软件打不开
        _scenarios["app_crash"] =
        [
            new() { Id = "reinstall_framework", Name = "修复 VC++ 运行库", Description = "检测并修复 Microsoft Visual C++ 运行库", Command = "", RiskLevel = RiskLevel.Safe },
            new() { Id = "corrupt_profile", Name = "重置应用配置", Description = "删除损坏的应用配置文件", Command = "", RiskLevel = RiskLevel.Low }
        ];
    }

    public List<string> GetProblemCategories() =>
        ["system_slow", "network_issue", "bsod", "update_failed", "game_lag", "app_crash"];

    public List<RepairSolution> GetSolutionsForProblem(string problemId) =>
        _scenarios.TryGetValue(problemId, out var solutions) ? solutions : [];

    public bool ExecuteSolution(RepairSolution solution)
    {
        // TODO: 实际的系统命令执行
        return true;
    }

    public string GetDiagnosticInfo()
    {
        // TODO: 收集系统诊断信息
        return "诊断信息收集功能开发中...";
    }
}
