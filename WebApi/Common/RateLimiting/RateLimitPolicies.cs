namespace WebApi.Common.RateLimiting;

/// <summary>
/// Имена политик rate limiting. Используются в атрибутах
/// <c>[EnableRateLimiting(...)]</c> и при регистрации в Program.cs,
/// чтобы не разъезжались строковые литералы.
/// </summary>
public static class RateLimitPolicies
{
    public const string AuthLogin = "auth-login";
    public const string AuthRegister = "auth-register";
    public const string AuthRequestReset = "auth-request-reset";
    public const string AuthSubmitReset = "auth-submit-reset";
    public const string EmailResend = "email-resend";
}
