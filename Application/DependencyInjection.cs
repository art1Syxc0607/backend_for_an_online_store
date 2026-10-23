using Application.Behaviors;
using FluentValidation;
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
            // Выполняются "изнутри наружу" (снизу вверх):
            // первый зарегистрированный — самый внешний.
            // ═══════════════════════════════════════════

            // 1️⃣ Validation — снаружи кэша: невалидный запрос
            //     не должен ни читать, ни писать кэш.
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

            // 2️⃣ Logging — снаружи, видит всё
            //cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

            // 3️⃣ Performance — измеряет время
            //cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));

            // 4️⃣ Caching — проверяет кэш ДО handler
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));

            // 5️⃣ CacheInvalidation — инвалидирует ПОСЛЕ handler
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));

            //// 6️⃣ Transaction — оборачивает handler
            //cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        return services;
    }
}
