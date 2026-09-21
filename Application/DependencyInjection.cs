using Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // ═══════════════════════════════════════════
            // Порядок behaviors КРИТИЧЕН!
            // Выполняются "изнутри наружу" (снизу вверх)
            // ═══════════════════════════════════════════

            // 1️⃣ Logging — снаружи, видит всё
            //cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

            // 2️⃣ Performance — измеряет время
            //cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));

            // 3️⃣ Caching — проверяет кэш ДО handler
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));

            // 4️⃣ CacheInvalidation — инвалидирует ПОСЛЕ handler
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));

            // 5️⃣ Validation — внутри, перед handler
            //cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

            //// 6️⃣ Transaction — оборачивает handler
            //cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));



        return services;
    }
}