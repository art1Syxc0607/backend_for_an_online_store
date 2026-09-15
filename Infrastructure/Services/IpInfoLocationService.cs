// Infrastructure/Services/LocationService.cs
using Application.Interfaces;
using IPinfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Net;
using System.Net.Http.Json;

namespace Infrastructure.Services;

public class IpInfoLocationService : ILocationService
{
    private readonly IPinfoClient _client; // ← Внедряем через DI
    private readonly ILogger<IpInfoLocationService> _logger;

    public IpInfoLocationService(IPinfoClient client, ILogger<IpInfoLocationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<LocationInfoDto?> GetLocationByIpAsync(string ip, CancellationToken ct = default)
    {
        // Проверяем, что IP валидный, прежде чем идти в API
        if (string.IsNullOrWhiteSpace(ip) || ip == "Unknown" || !IPAddress.TryParse(ip, out _))
        {
            _logger.LogWarning("Invalid IP address: {IP}", ip);
            return null;
        }

        try
        {
            var response = await _client.IPApi.GetDetailsAsync(ip, ct);

            if (response == null)
            {
                _logger.LogWarning("Failed to get location for IP {IP}", ip);
                return null;
            }

            return new LocationInfoDto
            {
                Country = response.Country,
                City = response.City,
                Region = response.Region,
                Continent = response.Continent.Name,
                TimeZone = response.Timezone
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location for IP {Ip}", ip);
            return null;
        }
    }
}