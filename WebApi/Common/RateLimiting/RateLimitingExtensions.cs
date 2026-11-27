using Application.DTOs.Error;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace WebApi.Common.RateLimiting;

/// <summary>
/// Регистрация ASP.NET Core Rate Limiting (.NET 8, входит в shared framework).
///
/// Принципы:
///  - анонимные auth-эндпоинты ключуются по IP;
///  - авторизованные (resend email) — по userId;
///  - опции читаются на каждый запрос через IOptionsMonitor, поэтому их можно
///    переопределять в тестах и нагрузочных прогонах без пересборки;
///  - Enabled=false или PermitLimit &lt;= 0 отключают лимитер (политика "no limiter").
/// </summary>
public static class RateLimitingExtensions
{
    public const string RateLimitedErrorCode = "RATE_LIMITED";

    public static IServiceCollection AddAppRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitOptions>(
            configuration.GetSection(RateLimitOptions.SectionName));

        services.AddRateLimiter(options =>
        {
            // По умолчанию middleware отвечает 503, нам нужен 429.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = WriteRejectionAsync;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var rateOptions = GetOptions(context);
                if (!rateOptions.Enabled)
                    return RateLimitPartition.GetNoLimiter("global");

                return RateLimitPartition.GetFixedWindowLimiter(
                    IpKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateOptions.Global.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateOptions.Global.WindowSeconds),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddPolicy(RateLimitPolicies.AuthLogin, context =>
                SlidingWindow(context, RateLimitPolicies.AuthLogin, o => o.Login));

            options.AddPolicy(RateLimitPolicies.AuthRegister, context =>
                SlidingWindow(context, RateLimitPolicies.AuthRegister, o => o.Register));

            options.AddPolicy(RateLimitPolicies.AuthRequestReset, context =>
                SlidingWindow(context, RateLimitPolicies.AuthRequestReset, o => o.RequestReset));

            options.AddPolicy(RateLimitPolicies.AuthSubmitReset, context =>
                SlidingWindow(context, RateLimitPolicies.AuthSubmitReset, o => o.SubmitReset));

            // Эндпоинт авторизованный: ключ по пользователю, чтобы один аккаунт
            // не мог спамить письмами, и при этом соседние пользователи не страдали.
            options.AddPolicy(RateLimitPolicies.EmailResend, context =>
                SlidingWindow(context, RateLimitPolicies.EmailResend, o => o.ResendEmail, UserOrIpKey(context)));
        });

        return services;
    }

    private static RateLimitPartition<string> SlidingWindow(
        HttpContext context,
        string policy,
        Func<RateLimitOptions, RateLimitPolicyOptions> selector,
        string? key = null)
    {
        var rateOptions = GetOptions(context);
        var policyOptions = selector(rateOptions);
        var partitionKey = key ?? IpKey(context);

        if (!rateOptions.Enabled || policyOptions.PermitLimit <= 0 || policyOptions.WindowSeconds <= 0)
            return RateLimitPartition.GetNoLimiter(policy + ":" + partitionKey);

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = policyOptions.PermitLimit,
                Window = TimeSpan.FromSeconds(policyOptions.WindowSeconds),
                SegmentsPerWindow = Math.Clamp(policyOptions.WindowSeconds, 1, 6),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static RateLimitOptions GetOptions(HttpContext context) =>
        context.RequestServices
            .GetRequiredService<IOptionsMonitor<RateLimitOptions>>()
            .CurrentValue;

    private static int ResolveWindowSeconds(HttpContext context)
    {
        var options = GetOptions(context);
        var policyName = context.GetEndpoint()
            ?.Metadata.GetMetadata<EnableRateLimitingAttribute>()
            ?.PolicyName;

        var policyOptions = policyName switch
        {
            RateLimitPolicies.AuthLogin => options.Login,
            RateLimitPolicies.AuthRegister => options.Register,
            RateLimitPolicies.AuthRequestReset => options.RequestReset,
            RateLimitPolicies.AuthSubmitReset => options.SubmitReset,
            RateLimitPolicies.EmailResend => options.ResendEmail,
            _ => options.Global
        };

        return Math.Max(1, policyOptions.WindowSeconds);
    }

    private static void SetRetryAfter(HttpContext context, int seconds) =>
        context.Response.Headers.RetryAfter =
            Math.Max(1, seconds).ToString(CultureInfo.InvariantCulture);

    private static string IpKey(HttpContext context) =>
        "ip:" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown");

    private static string UserOrIpKey(HttpContext context)
    {
        var userId = context.User.FindFirst("userId")?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return userId is not null ? "user:" + userId : IpKey(context);
    }

    private static async ValueTask WriteRejectionAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;

        // Token bucket отдаёт RetryAfter через lease; sliding/fixed window — нет,
        // поэтому для них сообщаем длину окна политики.
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            SetRetryAfter(httpContext, (int)Math.Ceiling(retryAfter.TotalSeconds));
        else
            SetRetryAfter(httpContext, ResolveWindowSeconds(httpContext));

        httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("WebApi.RateLimiting")
            .LogWarning(
                "Rate limit exceeded: {Method} {Path} from {Ip}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Connection.RemoteIpAddress);

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        // Единый формат ошибки, как у ExceptionHandlingMiddleware.
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponseDto
        {
            Error = "Too many requests. Please retry later.",
            ErrorCode = RateLimitedErrorCode,
            StatusCode = StatusCodes.Status429TooManyRequests,
            Timestamp = DateTime.UtcNow,
            Path = httpContext.Request.Path.Value,
            TraceId = httpContext.TraceIdentifier
        }, cancellationToken);
    }
}
