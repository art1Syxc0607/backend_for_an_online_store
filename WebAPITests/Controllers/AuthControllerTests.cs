// WebAPITests/Controllers/AuthControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Application.Commands.User;
using Application.DTOs.User;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WebAPITests.Common;
using Xunit;

namespace WebAPITests.Controllers;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WhenValidData_ShouldReturn200()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@mail.com",
            UserName = "JohnDoe",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@mail.com");
        result!.UserName.Should().Be("JohnDoe");
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturn400()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@mail.com",
            UserName = "JohnDoe",
            Password = "Password123!"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", command);
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ShouldReturn200()
    {
        // Arrange
        await RegisterUser();

        var loginDto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WhenInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        await RegisterUser();

        var loginDto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WhenUnauthorized_ShouldReturn401()
    {
        // Arrange
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/auth/change-password", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WhenAuthorized_ShouldReturn204()
    {
        // Arrange
        await RegisterUser();
        var token = await GetAuthToken();

        var client = _client;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPassword123!"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/auth/change-password", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task RegisterUser()
    {
        var command = new RegisterCommand
        {
            Email = "test@mail.com",
            UserName = "JohnDoe",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", command);
    }

    private async Task<string> GetAuthToken()
    {
        var loginDto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result!.Token;
    }
}