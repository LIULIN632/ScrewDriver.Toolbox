using ScrewDriver.Toolbox.Models;

namespace ScrewDriver.Toolbox.Services;

/// <summary>
/// 设备信息获取服务
/// </summary>
public interface IDeviceInfoService
{
    DeviceModel GetDeviceInfo();
    HardwareStatus GetCpuStatus();
    HardwareStatus GetMemoryStatus();
    HardwareStatus GetDiskStatus();
    HardwareStatus GetSystemStatus();
}
