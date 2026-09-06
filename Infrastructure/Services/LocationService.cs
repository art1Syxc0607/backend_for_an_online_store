// Infrastructure/Services/LocationService.cs
using System.Net.Http.Json;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocationService> _logger;

    public LocationService(HttpClient httpClient, ILogger<LocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LocationInfo?> GetLocationByIpAsync(string ip, CancellationToken ct = default)
    {
        try
        {
            // ip-api.com — бесплатный, до 45 запросов в минуту
            var url = $"http://ip-api.com/json/{ip}?fields=status,message,country,city,regionName,continent,timezone,isp,query";

            var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(url, ct);

            if (response == null || response.Status != "success")
            {
                _logger.LogWarning("Failed to get location for IP {Ip}: {Message}", ip, response?.Message ?? "Unknown error");
                return null;
            }

            return new LocationInfo
            {
                Country = response.Country,
                City = response.City,
                Region = response.RegionName,
                Continent = response.Continent,
                TimeZone = response.Timezone,
                Isp = response.Isp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location for IP {Ip}", ip);
            return null;
        }
    }

    private class IpApiResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? RegionName { get; set; }
        public string? Continent { get; set; }
        public string? Timezone { get; set; }
        public string? Isp { get; set; }
        public string? Query { get; set; }
    }
}