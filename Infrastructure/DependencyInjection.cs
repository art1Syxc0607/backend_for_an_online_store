using Application.Interfaces;
using Infrastructure.Cleanup;
using Infrastructure.Data;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Payment;
using Infrastructure.Services.Payment.Strategies;
using Infrastructure.UnitOfWork;
using IPinfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection Extension(this IServiceCollection services, IConfiguration configuration)
    {
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(password))
        {
            connectionString += $";Password={password}";
        }
        else
        {
            throw new InvalidOperationException(
    "Password not found. Set DB_PASSWORD environment variable.");
        }

        //services.AddDbContext<AppDbContext>(options => // SQLite
        //    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<AppDbContext>(options => // PostgreSQL
        options.UseNpgsql(connectionString,
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // for current reauest
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentRequestService, CurrentRequestService>();

        // ✅ Добавляем HttpClientFactory в DI
        services.AddHttpClient<ILocationService, IpInfoLocationService>();


        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ILocationService, IpInfoLocationService>();
        services.AddScoped<IDeviceInfoService, DeviceInfoService>();

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUnitOfWork, Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();


        //service
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // cash
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // EmailToken generator
        services.AddScoped<ITokenGenerator, TokenGenerator>();

        // Payment
        // Strategy Pattern
        // ✅ Регистрируем стратегии
        services.AddScoped<IPaymentStrategy, CardPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, GooglePayPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, ApplePayPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, SBPPaymentStrategy>();

        // ✅ Регистрируем фабрику и сервис
        services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();
        services.AddScoped<IPaymentService, PaymentService>();


        // Email
        // Регистрация
        services.ConfigureOptions<SmtpSettingsConfigureOptions>();
        // фоновые сервисы for email
        // 1. Очередь - Singleton
        services.AddSingleton<IEmailBackgroundTaskQueue, EmailBackgroundTaskQueue>();

        // 2. Сервис - Singleton
        services.AddSingleton<EmailBackgroundService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // 3. Интерфейс для инъекции в handlers
        services.AddSingleton<IEmailBackgroundService>(provider =>
            provider.GetRequiredService<EmailBackgroundService>());

        // 4. Регистрация как HostedService
        services.AddHostedService(provider =>
            provider.GetRequiredService<EmailBackgroundService>());


        // ✅ Background Cleanup tasks
        services.AddScoped<ICleanupTask, PasswordResetCodesCleanupTask>();
        services.AddScoped<ICleanupTask, ExpiredOrdersCleanupTask>();

        // ✅ Планировщик
        services.AddHostedService<CleanupBackgroundService>();


        // location API
        services.ConfigureOptions<IpInfoOptionsConfigureOptions>();

        // ✅ Клиент создаётся через IOptions<IpInfoOptions>
        services.AddSingleton<IPinfoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<IpInfoOptions>>().Value;

            return new IPinfoClient.Builder()
                .AccessToken(options.ApiKey)  // ← уже из env variable!
                .Build();
        });

        services.AddScoped<ILocationService, IpInfoLocationService>();

        // for service deleting orders
        services.Configure<ExpiredOrdersOptions>(
            configuration.GetSection(ExpiredOrdersOptions.SectionName));

        return services;
    }
}
