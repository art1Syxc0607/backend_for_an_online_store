// Infrastructure/Options/IpInfoOptionsConfigureOptions.cs
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Options;

public class IpInfoOptionsConfigureOptions : IConfigureOptions<IpInfoOptions>
{
    private readonly IConfiguration _configuration;

    public IpInfoOptionsConfigureOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(IpInfoOptions options)
    {
        // 1. Биндим из конфига
        _configuration.GetSection(IpInfoOptions.SectionName).Bind(options);

        // 2. ✅ ApiKey из env variable
        var apiKey = Environment.GetEnvironmentVariable("IPINFO_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            options.ApiKey = apiKey;
        }

        // 3. ✅ BaseUrl из env variable (для тестов)
        var baseUrl = Environment.GetEnvironmentVariable("IPINFO_BASE_URL");
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            options.BaseUrl = baseUrl;
        }

        // 4. Fail fast
        if (options.Enabled && string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "IpInfo:ApiKey is not configured. " +
                "Set IPINFO_API_KEY environment variable " +
                "or IpInfo:ApiKey in appsettings.json.");
        }
    }
}