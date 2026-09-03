using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Payment;
using Infrastructure.Services.Payment.Strategies;
using Infrastructure.UnitOfWork;
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

        //service
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // cash
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IEmailService, EmailService>();
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

        return services;
    }
}
