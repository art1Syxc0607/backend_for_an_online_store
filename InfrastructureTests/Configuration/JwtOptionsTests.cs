using System.Text;
using FluentAssertions;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InfrastructureTests.Configuration;

public class JwtOptionsTests
{
    private const string ValidKey = "01234567890123456789012345678901"; // 32 ASCII bytes

    [Fact]
    public void Load_WhenKeyIsMissing_ThrowsInvalidOperationException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        });

        var act = () => JwtOptions.Load(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT__KEY*");
    }

    [Fact]
    public void Load_WhenKeyIsTooShort_ThrowsInvalidOperationException()
    {
        var shortKey = new string('a', JwtOptions.MinKeyLengthBytes - 1);
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = shortKey,
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        });

        var act = () => JwtOptions.Load(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{JwtOptions.MinKeyLengthBytes} bytes*");
    }

    [Fact]
    public void Load_WhenKeyIsExactlyMinLength_ReturnsOptions()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = ValidKey,
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        });

        var options = JwtOptions.Load(configuration);

        options.Key.Should().Be(ValidKey);
        Encoding.UTF8.GetByteCount(options.Key).Should().Be(JwtOptions.MinKeyLengthBytes);
        options.Issuer.Should().Be("TestIssuer");
        options.Audience.Should().Be("TestAudience");
    }

    [Fact]
    public void Load_ReadsKeyFromEnvironmentVariable()
    {
        var previous = Environment.GetEnvironmentVariable("JWT__KEY");
        try
        {
            Environment.SetEnvironmentVariable("JWT__KEY", ValidKey);

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var options = JwtOptions.Load(configuration);

            options.Key.Should().Be(ValidKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT__KEY", previous);
        }
    }

    [Fact]
    public void Load_IsCaseInsensitiveForEnvironmentStyleKeys()
    {
        // The environment variable provider maps JWT__KEY to JWT:KEY, and
        // configuration keys are case-insensitive.
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["JWT:KEY"] = ValidKey,
            ["JWT:ISSUER"] = "TestIssuer",
            ["JWT:AUDIENCE"] = "TestAudience"
        });

        var options = JwtOptions.Load(configuration);

        options.Key.Should().Be(ValidKey);
        options.Issuer.Should().Be("TestIssuer");
        options.Audience.Should().Be("TestAudience");
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
