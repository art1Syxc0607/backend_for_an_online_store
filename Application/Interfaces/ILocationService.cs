using Application.DTOs.Location;

namespace Application.Interfaces;

public interface ILocationService
{
    Task<LocationInfoDto?> GetLocationByIpAsync(string ip, CancellationToken ct = default);
}

