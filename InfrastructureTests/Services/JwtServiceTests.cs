using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Options;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace InfrastructureTests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var options = Options.Create(new JwtOptions
        {
            Key = "test-only-jwt-signing-key-at-least-32-bytes!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        });

        _jwtService = new JwtService(options);
    }

    //[Fact]
    //public void GenerateToken_ShouldReturnValidToken()
    //{
    //    // Arrange
    //    var user = new User("test@mail.com", "hash", "John");
    //    user.TestsSetUser(1);

    //    // Act
    //    var token = _jwtService.GenerateToken(user);

    //    // Assert
    //    token.Should().NotBeNullOrEmpty();

    //    var handler = new JwtSecurityTokenHandler();
    //    var jsonToken = handler.ReadJwtToken(token);

    //    jsonToken.Should().NotBeNull();

    //    // ✅ Используем JwtRegisteredClaimNames (они совпадают с тем, что добавляет JwtService)
    //    jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email);
    //    jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name);
    //    jsonToken.Claims.Should().Contain(c => c.Type == "userId");
    //}

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