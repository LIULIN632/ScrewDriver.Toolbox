using ScrewDriver.Toolbox.Models;
using System.Management;

namespace ScrewDriver.Toolbox.Services;

public class DeviceInfoService : IDeviceInfoService
{
    public DeviceModel GetDeviceInfo()
    {
        var model = new DeviceModel();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                model.Manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                model.Model = obj["Model"]?.ToString() ?? "";
            }

            using var cpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in cpuSearcher.Get())
            {
                model.CpuName = obj["Name"]?.ToString()?.Trim() ?? "";
            }

            using var gpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var obj in gpuSearcher.Get())
            {
                model.GpuName = obj["Name"]?.ToString() ?? "";
                break; // 取第一个显卡
            }

            using var memSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (var obj in memSearcher.Get())
            {
                if (obj["TotalPhysicalMemory"] is long total)
                    model.MemoryGB = (int)(total / (1024 * 1024 * 1024));
            }

            model.WindowsVersion = Environment.OSVersion.VersionString;
            model.OsBuild = Environment.OSVersion.Version?.ToString() ?? "";
            model.UpTime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        }
        catch
        {
            // WMI 不可用时返回默认值
        }
        return model;
    }

    public HardwareStatus GetCpuStatus()
    {
        return new HardwareStatus
        {
            ComponentName = "CPU",
            Value = "正常",
            Unit = "",
            StatusColor = StatusColor.Normal
        };
    }

    public HardwareStatus GetMemoryStatus()
    {
        return new HardwareStatus
        {
            ComponentName = "内存",
            Value = "正常",
            Unit = "",
            StatusColor = StatusColor.Normal
        };
    }

    public HardwareStatus GetDiskStatus()
    {
        return new HardwareStatus
        {
            ComponentName = "磁盘",
            Value = "正常",
            Unit = "",
            StatusColor = StatusColor.Normal
        };
    }

    public HardwareStatus GetSystemStatus()
    {
        return new HardwareStatus
        {
            ComponentName = "系统",
            Value = "正常",
            Unit = "",
            StatusColor = StatusColor.Normal
        };
    }
}
