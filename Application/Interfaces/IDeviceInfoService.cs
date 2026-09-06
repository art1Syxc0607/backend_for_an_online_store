using Application.Interfaces;
using UAParser;

namespace Infrastructure.Services;

public interface IDeviceInfoService
{
    DeviceInfoDto GetDeviceInfo(string userAgent);
}

public class DeviceInfoDto
{
    public string Os { get; set; } = "Unknown";
    public string OsVersion { get; set; } = "Unknown";
    public string Browser { get; set; } = "Unknown";
    public string BrowserVersion { get; set; } = "Unknown";
    public string DeviceType { get; set; } = "Unknown";
    public string DeviceModel { get; set; } = "Unknown";
    public string FullInfo { get; set; } = "Unknown";
}