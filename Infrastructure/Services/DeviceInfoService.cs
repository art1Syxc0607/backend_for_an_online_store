using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace Infrastructure.Services;

public class DeviceInfoService : IDeviceInfoService
{
    private readonly Parser _parser;

    public DeviceInfoService()
    {
        _parser = Parser.GetDefault();
    }

    public DeviceInfoDto GetDeviceInfo(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return new DeviceInfoDto { FullInfo = "Unknown Device" };

        try
        {
            var clientInfo = _parser.Parse(userAgent);

            // Определяем тип устройства
            var deviceType = "Unknown";
            var deviceModel = "Unknown";

            if (clientInfo.Device.Family != "Other")
            {
                deviceType = clientInfo.Device.Family;
                deviceModel = clientInfo.Device.Model ?? "Unknown";
            }
            else if (clientInfo.UserAgent.Family.Contains("Windows", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = "Desktop";
            }
            else if (clientInfo.UserAgent.Family.Contains("Mac", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = "Desktop";
            }
            else if (clientInfo.UserAgent.Family.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                     clientInfo.UserAgent.Family.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = "Mobile";
            }
            else if (clientInfo.UserAgent.Family.Contains("Android", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = "Mobile";
            }

            // Формируем эмодзи
            var osEmoji = clientInfo.OS.Family.ToLower() switch
            {
                var os when os.Contains("windows") => "💻",
                var os when os.Contains("mac") => "🍎",
                var os when os.Contains("linux") => "🐧",
                var os when os.Contains("android") => "📱",
                var os when os.Contains("ios") || os.Contains("iphone") || os.Contains("ipad") => "🍏",
                _ => "🌐"
            };

            var fullInfo = $"{osEmoji} {clientInfo.OS.Family} {clientInfo.OS.Major} / {clientInfo.UA.Family} {clientInfo.UA.Major}";

            return new DeviceInfoDto
            {
                Os = clientInfo.OS.Family,
                OsVersion = clientInfo.OS.Major ?? "Unknown",
                Browser = clientInfo.UA.Family,
                BrowserVersion = clientInfo.UA.Major ?? "Unknown",
                DeviceType = deviceType,
                DeviceModel = deviceModel,
                FullInfo = fullInfo
            };
        }
        catch (Exception)
        {
            return new DeviceInfoDto { FullInfo = "Unknown Device" };
        }
    }
}
