using System.Management;
using System.Runtime.Versioning;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.Hardware.Services;

[SupportedOSPlatform("windows")]
public class HardwareService
{
    public static float GetTotalMemoryGB()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                if (obj["TotalPhysicalMemory"] is ulong bytes && bytes > 0)
                    return (float)Math.Round(bytes / (1024.0 * 1024 * 1024), 1);
            }
        }
        catch { }
        return 0;
    }

    public List<HardwareComponent> GetAllHardwareInfo()
    {
        var components = new List<HardwareComponent>
        {
            GetCpuInfo(),
            GetGpuInfo(),
            GetMemoryInfo(),
            GetDiskInfo()
        };

        var battery = GetBatteryInfo();
        if (battery != null) components.Add(battery);

        return components;
    }

    private static HardwareComponent GetCpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                var name = CleanCpuName(obj["Name"]?.ToString() ?? "未知 CPU");
                var cores = obj["NumberOfCores"]?.ToString() ?? "-";
                var threads = obj["NumberOfLogicalProcessors"]?.ToString() ?? "-";
                var freq = obj["MaxClockSpeed"] is uint mhz ? $"{mhz / 1000.0:F1} GHz" : "-";

                var threadInfo = threads == cores ? $"{cores} 核" : $"{cores} 核 / {threads} 线程";

                return new HardwareComponent
                {
                    Name = "CPU",
                    Icon = "\U0001f9e0",
                    PrimaryInfo = name,
                    SecondaryInfo = $"{threadInfo} · {freq}",
                    HealthStatus = "正常",
                    HealthLevel = "normal",
                    TipText = name.Contains("i3") || name.Contains("Celeron") || name.Contains("Pentium")
                        ? "当前 CPU 适合日常办公，重度多任务建议升级"
                        : ""
                };
            }
        }
        catch { }

        return new HardwareComponent { Name = "CPU", Icon = "\U0001f9e0", PrimaryInfo = "无法检测", HealthStatus = "未知" };
    }

    private static HardwareComponent GetGpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, AdapterRAM, CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");
            var gpus = new List<(string name, string driver, string vram, string resolution)>();

            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;

                var driver = obj["DriverVersion"]?.ToString() ?? "";
                var ramBytes = obj["AdapterRAM"] as ulong? ?? 0;
                var vram = ramBytes > 0 ? FormatBytes((long)ramBytes) : "";
                var h = obj["CurrentHorizontalResolution"];
                var v = obj["CurrentVerticalResolution"];
                var res = h != null && v != null ? $"{h}×{v}" : "";

                gpus.Add((name, driver, vram, res));
            }

            if (gpus.Count == 0)
                return new HardwareComponent { Name = "GPU", Icon = "\U0001f3ae", PrimaryInfo = "未检测到独立显卡", HealthStatus = "正常" };

            var primary = gpus[0];
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(primary.resolution)) parts.Add(primary.resolution);
            if (!string.IsNullOrEmpty(primary.vram)) parts.Add(primary.vram);
            var secInfo = string.Join(" · ", parts);

            if (gpus.Count > 1)
                secInfo = $"主显卡 + {gpus.Count - 1} 个 · " + secInfo;

            return new HardwareComponent
            {
                Name = "GPU",
                Icon = "\U0001f3ae",
                PrimaryInfo = Truncate(primary.name, 50),
                SecondaryInfo = secInfo.Length > 0 ? secInfo : "已识别",
                HealthStatus = "正常",
                HealthLevel = "normal",
                TipText = gpus.Count == 1 && primary.name.Contains("Microsoft Basic")
                    ? "使用微软基本显示适配器，建议安装专用显卡驱动"
                    : ""
            };
        }
        catch { }

        return new HardwareComponent { Name = "GPU", Icon = "\U0001f3ae", PrimaryInfo = "无法检测", HealthStatus = "未知" };
    }

    private static HardwareComponent GetMemoryInfo()
    {
        try
        {
            ulong totalCapacity = 0;
            var stickCount = 0;

            using (var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, Manufacturer FROM Win32_PhysicalMemory"))
            {
                foreach (var obj in searcher.Get())
                {
                    var cap = obj["Capacity"] as ulong? ?? 0;
                    if (cap > 0)
                    {
                        totalCapacity += cap;
                        stickCount++;
                    }
                }
            }

            // Also try Win32_OperatingSystem for visible memory
            if (totalCapacity == 0)
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                {
                    if (obj["TotalVisibleMemorySize"] is ulong totalKb)
                        totalCapacity = totalKb * 1024;
                    break;
                }
            }

            if (totalCapacity == 0)
                return new HardwareComponent { Name = "内存", Icon = "\U0001f4ca", PrimaryInfo = "无法检测", HealthStatus = "未知" };

            var totalStr = FormatBytes((long)totalCapacity);

            string healthStatus;
            string healthLevel;
            string tip;

            var totalGB = totalCapacity / (1024.0 * 1024 * 1024);
            if (totalGB < 4)
            {
                healthStatus = "严重不足";
                healthLevel = "danger";
                tip = "内存严重不足，强烈建议升级至 8 GB 以上";
            }
            else if (totalGB < 8)
            {
                healthStatus = $"仅 {totalGB:F0} GB";
                healthLevel = "warning";
                tip = "建议升级至 16 GB 以获得流畅体验";
            }
            else if (totalGB < 16)
            {
                healthStatus = "基本够用";
                healthLevel = "normal";
                tip = "日常使用无压力，重度多任务建议 16 GB 以上";
            }
            else
            {
                healthStatus = "充足";
                healthLevel = "normal";
                tip = "";
            }

            var stickText = stickCount > 0 ? $"{stickCount} 个插槽 · " : "";

            return new HardwareComponent
            {
                Name = "内存",
                Icon = "\U0001f4ca",
                PrimaryInfo = totalStr,
                SecondaryInfo = $"{stickText}总计 {totalStr}",
                HealthStatus = healthStatus,
                HealthLevel = healthLevel,
                TipText = tip
            };
        }
        catch { }

        return new HardwareComponent { Name = "内存", Icon = "\U0001f4ca", PrimaryInfo = "无法检测", HealthStatus = "未知" };
    }

    private static HardwareComponent GetDiskInfo()
    {
        try
        {
            var disks = new List<(string model, string size, string interfaceType, string healthStatus, string healthLevel)>();

            using (var searcher = new ManagementObjectSearcher("SELECT Model, Size, InterfaceType, Status FROM Win32_DiskDrive"))
            {
                foreach (var obj in searcher.Get())
                {
                    var model = obj["Model"]?.ToString()?.Trim() ?? "未知磁盘";
                    var size = obj["Size"] as ulong? ?? 0;
                    var iface = obj["InterfaceType"]?.ToString() ?? "";
                    var status = obj["Status"]?.ToString() ?? "OK";

                    var dHealth = status == "OK" ? "normal" : "warning";
                    disks.Add((model, FormatBytes((long)size), iface, status, dHealth));
                }
            }

            // Get free space per logical disk
            long totalFree = 0;
            long totalSize = 0;
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    totalSize += drive.TotalSize;
                    totalFree += drive.TotalFreeSpace;
                }
            }

            if (disks.Count == 0)
                return new HardwareComponent { Name = "磁盘", Icon = "\U0001f4be", PrimaryInfo = "未检测到", HealthStatus = "未知" };

            var freePct = totalSize > 0 ? (int)(totalFree * 100 / totalSize) : -1;

            string healthStatus;
            string healthLevel;
            string tip;

            if (freePct is >= 0 and < 10)
            {
                healthStatus = $"可用仅 {freePct}%";
                healthLevel = "danger";
                tip = "磁盘空间严重不足！请立即清理或扩容";
            }
            else if (freePct is >= 10 and < 20)
            {
                healthStatus = $"可用 {freePct}%";
                healthLevel = "warning";
                tip = "磁盘空间偏低，建议清理临时文件和不常用软件";
            }
            else if (freePct >= 0)
            {
                healthStatus = $"可用 {freePct}%";
                healthLevel = "normal";
                tip = "";
            }
            else
            {
                healthStatus = "正常";
                healthLevel = "normal";
                tip = "";
            }

            var infoStr = disks.Count == 1
                ? $"{disks[0].model}"
                : $"{disks.Count} 个磁盘";

            return new HardwareComponent
            {
                Name = "磁盘",
                Icon = "\U0001f4be",
                PrimaryInfo = infoStr,
                SecondaryInfo = freePct >= 0 ? $"可用空间 {freePct}%" : "",
                HealthStatus = healthStatus,
                HealthLevel = healthLevel,
                TipText = tip
            };
        }
        catch { }

        return new HardwareComponent { Name = "磁盘", Icon = "\U0001f4be", PrimaryInfo = "无法检测", HealthStatus = "未知" };
    }

    private static HardwareComponent? GetBatteryInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining, BatteryStatus, EstimatedRunTime FROM Win32_Battery");
            foreach (var obj in searcher.Get())
            {
                var charge = obj["EstimatedChargeRemaining"] as ushort? ?? 0;
                var status = (obj["BatteryStatus"] as ushort? ?? 0) switch
                {
                    1 => "电池放电中",
                    2 => "接通电源",
                    3 => "已充满",
                    4 => "电量低",
                    5 => "电量临界",
                    6 => "充电中",
                    7 => "充电中",
                    8 => "充电中",
                    _ => "未知"
                };
                var runtime = obj["EstimatedRunTime"] is ushort mins && mins > 0 && mins != ushort.MaxValue ? $"{mins} 分钟" : "";

                var healthLevel = charge switch
                {
                    < 20 => "danger",
                    < 50 => "warning",
                    _ => "normal"
                };

                var health = charge switch
                {
                    < 20 => "电量极低",
                    < 50 => "电量偏低",
                    _ => "正常"
                };

                var secParts = new List<string> { status };
                if (!string.IsNullOrEmpty(runtime)) secParts.Add($"预计剩余 {runtime}");

                return new HardwareComponent
                {
                    Name = "电池",
                    Icon = "\U0001f50b",
                    PrimaryInfo = $"{charge}%",
                    SecondaryInfo = string.Join(" · ", secParts),
                    HealthStatus = health,
                    HealthLevel = healthLevel,
                    TipText = charge < 20 ? "电量极低，请尽快连接电源" : ""
                };
            }
        }
        catch { }

        return null;
    }

    public List<HardwareDetailModule> GetAllDetailInfo()
    {
        var modules = new List<HardwareDetailModule>
        {
            GetCpuDetail(),
            GetMotherboardDetail(),
            GetMemoryDetail(),
            GetGpuDetail(),
            GetDiskDetail(),
            GetMonitorDetail(),
            GetAudioDetail(),
            GetNetworkDetail()
        };
        return modules;
    }

    private static HardwareDetailModule GetCpuDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, L2CacheSize, L3CacheSize, SocketDesignation FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                items.Add(new() { Label = "型号", Value = CleanCpuName(obj["Name"]?.ToString() ?? "未知") });
                items.Add(new() { Label = "制造商", Value = obj["Manufacturer"]?.ToString() ?? "未知" });
                items.Add(new() { Label = "物理核心", Value = obj["NumberOfCores"]?.ToString() ?? "-" });
                items.Add(new() { Label = "逻辑线程", Value = obj["NumberOfLogicalProcessors"]?.ToString() ?? "-" });
                var freq = obj["MaxClockSpeed"] is uint mhz ? $"{mhz / 1000.0:F1} GHz" : "-";
                items.Add(new() { Label = "基础频率", Value = freq });
                items.Add(new() { Label = "插槽", Value = obj["SocketDesignation"]?.ToString() ?? "-" });
                items.Add(new() { Label = "二级缓存", Value = obj["L2CacheSize"] != null ? $"{obj["L2CacheSize"]} KB" : "-" });
                items.Add(new() { Label = "三级缓存", Value = obj["L3CacheSize"] != null ? $"{obj["L3CacheSize"]} KB" : "-" });
            }
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "处理器", Icon = "\U0001f9e0", Items = items };
    }

    private static HardwareDetailModule GetMotherboardDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var board = new ManagementObjectSearcher("SELECT Manufacturer, Product, Version, SerialNumber FROM Win32_BaseBoard");
            foreach (var obj in board.Get())
            {
                items.Add(new() { Label = "制造商", Value = obj["Manufacturer"]?.ToString() ?? "未知" });
                items.Add(new() { Label = "型号", Value = obj["Product"]?.ToString() ?? "未知" });
                items.Add(new() { Label = "版本", Value = obj["Version"]?.ToString() ?? "-" });
                items.Add(new() { Label = "序列号", Value = obj["SerialNumber"]?.ToString() ?? "-" });
            }

            using var bios = new ManagementObjectSearcher("SELECT SMBIOSBIOSVersion, ReleaseDate, Manufacturer FROM Win32_BIOS");
            foreach (var obj in bios.Get())
            {
                items.Add(new() { Label = "BIOS 版本", Value = obj["SMBIOSBIOSVersion"]?.ToString() ?? "-" });
                items.Add(new() { Label = "BIOS 厂商", Value = obj["Manufacturer"]?.ToString() ?? "-" });
                if (obj["ReleaseDate"] is string date && date.Length >= 8)
                    items.Add(new() { Label = "BIOS 日期", Value = $"{date[..4]}-{date[4..6]}-{date[6..8]}" });
            }

            using var system = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (var obj in system.Get())
            {
                items.Insert(0, new() { Label = "系统机型", Value = $"{obj["Manufacturer"]} {obj["Model"]}" });
            }
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "主板", Icon = "\U0001f50c", Items = items };
    }

    private static HardwareDetailModule GetMemoryDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            ulong totalCapacity = 0;
            var slotIndex = 0;
            using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, Manufacturer, FormFactor, MemoryType FROM Win32_PhysicalMemory");
            foreach (var obj in searcher.Get())
            {
                var cap = obj["Capacity"] as ulong? ?? 0;
                if (cap > 0)
                {
                    totalCapacity += cap;
                    slotIndex++;
                    var speed = obj["Speed"]?.ToString() ?? "-";
                    var manufacturer = obj["Manufacturer"]?.ToString() ?? "未知";
                    items.Add(new() { Label = $"插槽 {slotIndex}", Value = $"{FormatBytes((long)cap)}  {manufacturer}  {speed} MHz" });
                }
            }

            if (totalCapacity > 0)
            {
                items.Insert(0, new() { Label = "总容量", Value = FormatBytes((long)totalCapacity) });
                items.Add(new() { Label = "插槽数量", Value = $"{slotIndex} 个" });
            }

            if (items.Count == 0)
                items.Add(new() { Label = "状态", Value = "无法检测" });
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "内存", Icon = "\U0001f4ca", Items = items };
    }

    private static HardwareDetailModule GetGpuDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, DriverDate, AdapterRAM, CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate, AdapterCompatibility FROM Win32_VideoController");
            var gpuIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;
                gpuIndex++;
                var prefix = gpuIndex > 1 ? $"显卡 {gpuIndex}" : "显卡";
                items.Add(new() { Label = $"{prefix} 型号", Value = name });
                items.Add(new() { Label = $"{prefix} 厂商", Value = obj["AdapterCompatibility"]?.ToString() ?? "-" });
                var ramBytes = obj["AdapterRAM"] as ulong? ?? 0;
                if (ramBytes > 0)
                    items.Add(new() { Label = $"{prefix} 显存", Value = FormatBytes((long)ramBytes) });
                items.Add(new() { Label = $"{prefix} 驱动版本", Value = obj["DriverVersion"]?.ToString() ?? "-" });
                if (obj["DriverDate"] is string date && date.Length >= 8)
                    items.Add(new() { Label = $"{prefix} 驱动日期", Value = $"{date[..4]}-{date[4..6]}-{date[6..8]}" });
                var h = obj["CurrentHorizontalResolution"];
                var v = obj["CurrentVerticalResolution"];
                var r = obj["CurrentRefreshRate"];
                if (h != null && v != null)
                    items.Add(new() { Label = $"{prefix} 分辨率", Value = $"{h}×{v}" });
                if (r != null)
                    items.Add(new() { Label = $"{prefix} 刷新率", Value = $"{r} Hz" });
            }
            if (gpuIndex == 0)
                items.Add(new() { Label = "状态", Value = "未检测到" });
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "显卡", Icon = "\U0001f3ae", Items = items };
    }

    private static HardwareDetailModule GetDiskDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model, Size, InterfaceType, MediaType, SerialNumber FROM Win32_DiskDrive");
            var diskIndex = 0;
            foreach (var obj in searcher.Get())
            {
                diskIndex++;
                var model = obj["Model"]?.ToString()?.Trim() ?? "未知";
                var size = obj["Size"] as ulong? ?? 0;
                var iface = obj["InterfaceType"]?.ToString() ?? "-";
                var media = obj["MediaType"]?.ToString() ?? "-";
                var sn = obj["SerialNumber"]?.ToString()?.Trim() ?? "-";

                items.Add(new() { Label = $"磁盘 {diskIndex} 型号", Value = model });
                if (size > 0) items.Add(new() { Label = $"磁盘 {diskIndex} 容量", Value = FormatBytes((long)size) });
                items.Add(new() { Label = $"磁盘 {diskIndex} 接口", Value = iface });
                items.Add(new() { Label = $"磁盘 {diskIndex} 序列号", Value = sn });
            }

            // Logical drive free space
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    var freePct = drive.TotalSize > 0 ? (int)(drive.TotalFreeSpace * 100 / drive.TotalSize) : 0;
                    items.Add(new() { Label = $"{drive.Name} 可用空间", Value = $"{FormatBytes(drive.TotalFreeSpace)} ({freePct}%)" });
                }
            }

            if (diskIndex == 0)
                items.Add(new() { Label = "状态", Value = "未检测到" });
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "硬盘", Icon = "\U0001f4be", Items = items };
    }

    private static HardwareDetailModule GetMonitorDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, ScreenWidth, ScreenHeight, DisplayFrequency FROM Win32_DisplayDevice");
            var monIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;
                monIndex++;
                items.Add(new() { Label = $"显示器 {monIndex}", Value = name });
                var w = obj["ScreenWidth"]?.ToString() ?? "-";
                var h = obj["ScreenHeight"]?.ToString() ?? "-";
                items.Add(new() { Label = $"分辨率", Value = $"{w}×{h}" });
                var freq = obj["DisplayFrequency"]?.ToString() ?? "-";
                items.Add(new() { Label = $"刷新率", Value = $"{freq} Hz" });
            }
            if (monIndex == 0) items.Add(new() { Label = "状态", Value = "未检测到" });
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "显示器", Icon = "\U0001f5a5", Items = items };
    }

    private static HardwareDetailModule GetAudioDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer, Status FROM Win32_SoundDevice");
            var idx = 0;
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;
                idx++;
                items.Add(new() { Label = $"声卡 {idx}", Value = name });
                items.Add(new() { Label = $"状态", Value = obj["Status"]?.ToString() ?? "正常" });
            }
            if (idx == 0) items.Add(new() { Label = "状态", Value = "未检测到" });
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "声卡", Icon = "\U0001f50a", Items = items };
    }

    private static HardwareDetailModule GetNetworkDetail()
    {
        var items = new List<HardwareDetailItem>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, MACAddress, Speed, NetEnabled, AdapterType FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True");
            var idx = 0;
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(name)) continue;
                idx++;
                items.Add(new() { Label = $"网卡 {idx}", Value = name });
                var mac = obj["MACAddress"]?.ToString() ?? "-";
                items.Add(new() { Label = $"MAC 地址", Value = mac });
                var speed = obj["Speed"] as ulong? ?? 0;
                if (speed > 0)
                    items.Add(new() { Label = $"速率", Value = speed >= 1_000_000_000 ? $"{speed / 1_000_000_000} Gbps" : $"{speed / 1_000_000} Mbps" });
                var enabled = obj["NetEnabled"] as bool?;
                items.Add(new() { Label = $"状态", Value = enabled == true ? "已启用" : "已禁用" });
            }
            if (idx == 0)
            {
                // Fallback: query all net adapters
                using var fallback = new ManagementObjectSearcher("SELECT Name, MACAddress FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL");
                foreach (var obj in fallback.Get())
                {
                    idx++;
                    items.Add(new() { Label = $"网卡 {idx}", Value = obj["Name"]?.ToString() ?? "未知" });
                    items.Add(new() { Label = $"MAC 地址", Value = obj["MACAddress"]?.ToString() ?? "-" });
                }
            }
            if (idx == 0) items.Add(new() { Label = "状态", Value = "未检测到" });
        }
        catch { items.Add(new() { Label = "状态", Value = "无法检测" }); }
        return new HardwareDetailModule { ModuleName = "网卡", Icon = "\U0001f4e1", Items = items };
    }

    public static string GetSystemUptime()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                if (obj["LastBootUpTime"] is string dmtf && dmtf.Length >= 14)
                {
                    var boot = new DateTime(
                        int.Parse(dmtf[..4]), int.Parse(dmtf[4..6]), int.Parse(dmtf[6..8]),
                        int.Parse(dmtf[8..10]), int.Parse(dmtf[10..12]), int.Parse(dmtf[12..14]));
                    var uptime = DateTime.Now - boot;
                    if (uptime.TotalDays >= 1)
                        return $"{(int)uptime.TotalDays} 天 {uptime.Hours} 小时 {uptime.Minutes} 分钟";
                    return $"{uptime.Hours} 小时 {uptime.Minutes} 分钟";
                }
            }
        }
        catch { }
        // Fallback: Environment.TickCount
        var ms = Environment.TickCount64;
        var days = ms / 86400000;
        var hours = (ms % 86400000) / 3600000;
        return $"{days} 天 {hours} 小时";
    }

    private static string CleanCpuName(string raw)
    {
        // Remove (R), (TM), (C), (r), (tm), (c) markers
        var cleaned = System.Text.RegularExpressions.Regex.Replace(raw, @"\s*\((R|TM|C|r|tm|c)\)\s*", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // Collapse multiple spaces
        while (cleaned.Contains("  ")) cleaned = cleaned.Replace("  ", " ");
        // Remove trailing "CPU @ x.xxGHz" noise since we show frequency separately
        var atIndex = cleaned.IndexOf(" @", StringComparison.Ordinal);
        if (atIndex > 0) cleaned = cleaned[..atIndex];
        return cleaned.Trim();
    }

    private static string Truncate(string text, int maxLen)
        => text.Length <= maxLen ? text : text[..(maxLen - 3)] + "...";

    private static string FormatBytes(long bytes) => bytes switch
    {
        >= 1_099_511_627_776 => $"{bytes / 1_099_511_627_776.0:F1} TB",
        >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F0} GB",
        >= 1_048_576 => $"{bytes / 1_048_576.0:F0} MB",
        _ => $"{bytes / 1024.0:F0} KB"
    };
}
