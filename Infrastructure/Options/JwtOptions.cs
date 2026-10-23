using System.Text;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinKeyLengthBytes = 32;

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Reads the JWT section from the combined configuration (appsettings.json,
    /// environment variables such as JWT__KEY, user-secrets, command line) and
    /// validates the signing key before the host is built.
    /// </summary>
    public static JwtOptions Load(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        var options = new JwtOptions
        {
            Key = section["Key"] ?? string.Empty,
            Issuer = section["Issuer"] ?? string.Empty,
            Audience = section["Audience"] ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. " +
                "Set the JWT__KEY environment variable (at least 32 bytes) " +
                "or use user-secrets for local development.");
        }

        if (Encoding.UTF8.GetByteCount(options.Key) < MinKeyLengthBytes)
        {
            throw new InvalidOperationException(
                $"JWT signing key must be at least {MinKeyLengthBytes} bytes (256 bits).");
        }

        return options;
    }
}
