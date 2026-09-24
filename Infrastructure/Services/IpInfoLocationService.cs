// Infrastructure/Services/IpInfo/IpInfoLocationService.cs
using Application.DTOs.Location;
using Application.Interfaces;
using Infrastructure.Options;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace Infrastructure.Services.IpInfo;

public class IpInfoLocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly IpInfoOptions _options;
    private readonly ILogger<IpInfoLocationService> _logger;

    public IpInfoLocationService(
        HttpClient httpClient,
        IOptions<IpInfoOptions> options,
        ILogger<IpInfoLocationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LocationInfoDto?> GetLocationByIpAsync(
        string ip,
        CancellationToken ct = default)
    {
        // 0. Сервис отключён?
        if (!_options.Enabled)
        {
            _logger.LogDebug("IpInfo service is disabled");
            return null;
        }

        // 1. Валидация IP
        if (string.IsNullOrWhiteSpace(ip) ||
            ip == "Unknown" ||
            !IPAddress.TryParse(ip, out var parsedIp))
        {
            _logger.LogWarning("Invalid IP address: {IP}", ip);
            return null;
        }

        // 2. ✅ Пропускаем loopback (::1, 127.0.0.1)
        if (IPAddress.IsLoopback(parsedIp))
        {
            _logger.LogDebug(
                "Skipping location lookup for loopback IP: {IP}", ip);
            return null;
        }

        // 3. ✅ Пропускаем приватные IP (10.x, 192.168.x, 172.16-31.x)
        if (IsPrivateIp(parsedIp))
        {
            _logger.LogDebug(
                "Skipping location lookup for private IP: {IP}", ip);
            return null;
        }

        try
        {
            // 4. ✅ Прямой HTTP-запрос
            var url = $"{_options.BaseUrl}/{ip}/json?token={_options.ApiKey}";

            _logger.LogDebug("Requesting IP info for {IP}", ip);

            var response = await _httpClient.GetFromJsonAsync<IpInfoResponse>(
                url, ct);

            if (response == null)
            {
                _logger.LogWarning(
                    "Empty response from IPinfo for IP {IP}", ip);
                return null;
            }

            // 5. ✅ Проверяем bogon (приватный IP)
            if (response.Bogon == true)
            {
                _logger.LogDebug(
                    "IP {IP} is bogon (private), skipping", ip);
                return null;
            }

            // 6. ✅ Парсим ISP из org
            var isp = ParseIsp(response.Org);

            _logger.LogDebug(
                    "Location Is recieved by ip:{ip}", ip);

            return new LocationInfoDto
            {
                Country = response.Country,
                City = response.City,
                Region = response.Region,
                TimeZone = response.Timezone,
                Isp = isp
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex,
                "HTTP error getting location for IP {IP}: {Message}",
                ip, ex.Message);
            return null;
        }
        catch (TaskCanceledException ex) when (ct.IsCancellationRequested)
        {
            // Запрос отменён клиентом — нормально
            _logger.LogDebug("Request cancelled for IP {IP}", ip);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            // Таймаут
            _logger.LogWarning(ex,
                "Timeout getting location for IP {IP}", ip);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error getting location for IP {IP}", ip);
            return null;
        }
    }

    /// <summary>
    /// Проверяет, является ли IP приватным.
    /// </summary>
    private static bool IsPrivateIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
            return true;

        // IPv4 private ranges
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10) return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168) return true;

            // 169.254.0.0/16 (link-local)
            if (bytes[0] == 169 && bytes[1] == 254) return true;
        }

        // IPv6 unique local (fc00::/7)
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = ip.GetAddressBytes();
            if ((bytes[0] & 0xFE) == 0xFC) return true;
        }

        return false;
    }

    /// <summary>
    /// Извлекает ISP из поля org: "AS15169 Google LLC" → "Google LLC".
    /// </summary>
    private static string? ParseIsp(string? org)
    {
        if (string.IsNullOrWhiteSpace(org))
            return null;

        // org = "AS15169 Google LLC"
        var spaceIndex = org.IndexOf(' ');

        return spaceIndex > 0 && spaceIndex < org.Length - 1
            ? org[(spaceIndex + 1)..].Trim()
            : org;
    }
}