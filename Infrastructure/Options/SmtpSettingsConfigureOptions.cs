using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

public class SmtpSettingsConfigureOptions : IConfigureOptions<SmtpSettings>
{
    private readonly IConfiguration _configuration;

    public SmtpSettingsConfigureOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SmtpSettings options)
    {
        // 1. Биндим из конфига (все поля, кроме пароля)
        _configuration.GetSection("Smtp").Bind(options);

        // 2. ✅ Явно берём пароль из env variable
        var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        if (!string.IsNullOrEmpty(password))
        {
            options.Password = password;
        }
    }
}

