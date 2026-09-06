using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentRequestService : ICurrentRequestService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentRequestService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetClientIp()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "Unknown";

        var forwardedIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedIp))
            return forwardedIp.Split(',').First().Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public string GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }
}