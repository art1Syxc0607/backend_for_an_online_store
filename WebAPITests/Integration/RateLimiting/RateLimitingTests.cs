// WebAPITests/Integration/RateLimiting/RateLimitingTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs.Error;
using Application.DTOs.User;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Common.RateLimiting;
using WebAPITests.Infrastructure;

public class RateLimitingTests : IClassFixture<RateLimitedWebApplicationFactory>
{
    private readonly RateLimitedWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RateLimitingTests(RateLimitedWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WhenLimitExceeded_Returns429AndKeepsOtherEndpointsWorking()
    {
        // Arrange
        const string email = "rate-limit@mail.com";
        const string password = "Test123!@#";

        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            UserName = "RateLimitUser"
        });
        register.EnsureSuccessStatusCode();

        await ConfirmEmailAsync(email);

        // Act - лимит логина в тестовой фабрике = 2 запроса в минуту
        var first = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        first.EnsureSuccessStatusCode();
        var auth = await first.Content.ReadFromJsonAsync<AuthResponseDto>();
        auth!.Token.Should().NotBeNullOrWhiteSpace();

        var second = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        second.EnsureSuccessStatusCode();

        var rejected = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });

        // Assert
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.Should().NotBeNull(
            "клиент должен знать, через сколько секунд повторить запрос");

        var error = await rejected.Content.ReadFromJsonAsync<ErrorResponseDto>();
        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be(RateLimitingExtensions.RateLimitedErrorCode);
        error.StatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
        error.Path.Should().Be("/api/auth/login");
        error.TraceId.Should().NotBeNullOrWhiteSpace();

        // Другая политика не расходуется: профиль по токену первого логина работает.
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var profile = await _client.SendAsync(request);
        profile.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task ConfirmEmailAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.FirstAsync(u => u.Email == email);
        var token = user.EmailConfirmationToken
            ?? throw new InvalidOperationException($"User {email} has no confirmation token.");

        user.ConfirmEmail(token);
        await db.SaveChangesAsync();
    }
}
