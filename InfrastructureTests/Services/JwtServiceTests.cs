using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InfrastructureTests.Services;

public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var configDict = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "mysupersecretkeywithmorethan32characters!",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        jsonToken.Should().NotBeNull();

        // ✅ Используем JwtRegisteredClaimNames (они совпадают с тем, что добавляет JwtService)
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name);
        jsonToken.Claims.Should().Contain(c => c.Type == "userId");
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldContainUserIdClaim()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(2);

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId");
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(user.Id.ToString());
    }
}