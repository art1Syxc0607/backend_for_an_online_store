namespace WebApi.Common.RateLimiting;

/// <summary>
/// Настройки rate limiting из секции "RateLimiting".
/// Значения можно переопределять через конфигурацию/переменные окружения
/// (например, RateLimiting__Login__PermitLimit).
/// </summary>
public class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Глобальный выключатель. В интеграционных и нагрузочных тестах
    /// выключается, чтобы лимиты не искажали результат.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Мягкий лимит на весь API: только против флуда.</summary>
    public RateLimitPolicyOptions Global { get; set; } = new()
    {
        PermitLimit = 600,
        WindowSeconds = 60
    };

    /// <summary>POST /api/auth/login</summary>
    public RateLimitPolicyOptions Login { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 60
    };

    /// <summary>POST /api/auth/register</summary>
    public RateLimitPolicyOptions Register { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 600
    };

    /// <summary>POST /api/auth/change-password (запрос кода на email)</summary>
    public RateLimitPolicyOptions RequestReset { get; set; } = new()
    {
        PermitLimit = 3,
        WindowSeconds = 900
    };

    /// <summary>POST /api/auth/reset-password (проверка кода)</summary>
    public RateLimitPolicyOptions SubmitReset { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 900
    };

    /// <summary>GET /api/auth/resend-confirmation-email</summary>
    public RateLimitPolicyOptions ResendEmail { get; set; } = new()
    {
        PermitLimit = 1,
        WindowSeconds = 300
    };
}

public class RateLimitPolicyOptions
{
    /// <summary>Сколько запросов разрешено в окне. &lt;= 0 отключает политику.</summary>
    public int PermitLimit { get; set; } = 60;

    /// <summary>Длина окна в секундах.</summary>
    public int WindowSeconds { get; set; } = 60;
}
