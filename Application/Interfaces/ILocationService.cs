namespace Application.Interfaces;

public interface ILocationService
{
    Task<LocationInfo?> GetLocationByIpAsync(string ip, CancellationToken ct = default);
}

public class LocationInfo
{
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Continent { get; set; }
    public string? TimeZone { get; set; }
    public string? Isp { get; set; }
}