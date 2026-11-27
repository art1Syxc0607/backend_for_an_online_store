// WebAPITests/Infrastructure/RateLimitedWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebAPITests.Infrastructure;

/// <summary>
/// Фабрика с включённым rate limiting и низким лимитом логина,
/// чтобы можно было детерминированно проверить 429.
/// </summary>
public class RateLimitedWebApplicationFactory : CustomWebApplicationFactory
{
    public const int LoginPermitLimit = 2;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // In-memory источник добавляется последним и перекрывает
            // RateLimiting:Enabled=false из базовой фабрики.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:Enabled"] = "true",
                ["RateLimiting:Global:PermitLimit"] = "10000",
                ["RateLimiting:Global:WindowSeconds"] = "60",
                ["RateLimiting:Login:PermitLimit"] = LoginPermitLimit.ToString(),
                ["RateLimiting:Login:WindowSeconds"] = "60",
                ["RateLimiting:Register:PermitLimit"] = "10",
                ["RateLimiting:Register:WindowSeconds"] = "600"
            });
        });
    }
}
