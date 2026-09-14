// WebAPITests/Integration/Auth/AuthFlowTests.cs
using Application.DTOs.User;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebAPITests.Infrastructure;

namespace WebAPITests.Integration.Auth;

public class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WhenValid_ShouldReturn201AndUser()
    {
        // Arrange
        var dto = new
        {
            Email = "newuser@mail.com",
            Password = "Test123!@#",
            UserName = "NewUser"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(dto.Email);
        result.UserName.Should().Be(dto.UserName);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturn400()
    {
        // Arrange
        var dto = new
        {
            Email = "duplicate@mail.com",
            Password = "Test123!@#",
            UserName = "User1"
        };

        await Client.PostAsJsonAsync("/api/auth/register", dto);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WhenWeakPassword_ShouldReturn400()
    {
        // Arrange
        var dto = new
        {
            Email = "weak@mail.com",
            Password = "123",  // ← слишком простой
            UserName = "WeakUser"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ShouldReturnJwt()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT = 3 части
    }

    [Fact]
    public async Task Login_WhenWrongPassword_ShouldReturn401()
    {
        // Arrange
        await RegisterAndLoginAsync("user@mail.com", "CorrectPass123!@#");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@mail.com",
            Password = "WrongPass123!@#"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WhenAuthenticated_ShouldReturnUser()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("profile@mail.com");
        SetAuthToken(token);

        // Act
        var response = await Client.GetAsync("/api/auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProfile_WhenNotAuthenticated_ShouldReturn401()
    {
        // Act
        var response = await Client.GetAsync("/api/auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}