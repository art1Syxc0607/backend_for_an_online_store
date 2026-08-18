using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Payment.Strategies;
using Infrastructure.Services.Payment;
using Infrastructure.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        //services.AddDbContext<AppDbContext>(options =>
        //    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));


        services.AddScoped<ICartRepository, CartRepository>();
        //services.AddScoped<ICategoryRepository, CategoryRepository>
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        //services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUnitOfWork, Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        // cash
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Payment
        // Strategy Pattern
        // ✅ Регистрируем стратегии
        services.AddScoped<IPaymentStrategy, CardPaymentStrategy>();
        //services.AddScoped<IPaymentStrategy, GooglePayPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, ApplePayPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, SBPPaymentStrategy>();

        // ✅ Регистрируем фабрику и сервис
        services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
