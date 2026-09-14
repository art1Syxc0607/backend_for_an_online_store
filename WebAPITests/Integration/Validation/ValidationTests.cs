// WebAPITests/Integration/Validation/ValidationTests.cs
using System.Net;
using WebAPITests.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

public class ValidationTests : IntegrationTestBase
{
    public ValidationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Theory]
    [InlineData("", "Password123!@#", "User")]        // пустой email
    [InlineData("invalid-email", "Password123!@#", "User")] // плохой email
    [InlineData("test@mail.com", "", "User")]         // пустой пароль
    [InlineData("test@mail.com", "123", "User")]      // короткий пароль
    [InlineData("test@mail.com", "Password123!@#", "")] // пустой username
    public async Task Register_WhenInvalidData_ShouldReturn400(
        string email, string password, string userName)
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            UserName = userName
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WhenEmptyItems_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/order", new
        {
            ShippingAddress = "ул. Ленина, д. 1",
            Items = Array.Empty<object>()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}