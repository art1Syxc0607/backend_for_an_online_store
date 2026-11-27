// WebAPITests/Infrastructure/CustomWebApplicationFactory.cs
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.PostgreSql;

namespace WebAPITests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Program.cs validates JWT configuration before the host is built, so the
    // test key must be visible as an environment variable before CreateClient().
    private const string TestJwtKey = "integration-test-jwt-signing-key-32+chars";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("webstore_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("JWT__KEY", TestJwtKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Rate limiting по умолчанию выключен: счётчики живут в памяти процесса
        // и не сбрасываются Respawn между тестами, поэтому лимиты ломали бы прогон.
        // Для проверки самих лимитов используется RateLimitedWebApplicationFactory.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimiting:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // ✅ Удаляем реальный DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // ✅ Удаляем все регистрации AppDbContext
            var dbContextDescriptors = services
                .Where(d => d.ServiceType == typeof(AppDbContext) ||
                            d.ServiceType == typeof(DbContextOptions))
                .ToList();
            foreach (var d in dbContextDescriptors)
                services.Remove(d);

            // ✅ Подключаем Testcontainer
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        // ✅ Запускаем контейнер
        await _dbContainer.StartAsync();

        // ✅ Применяем миграции до старта хоста, чтобы сидирование в Program.cs
        //    увидело готовую схему.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}