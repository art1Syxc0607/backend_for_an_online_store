using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Order;
using FluentAssertions;
using WebAPITests.Infrastructure;

namespace WebAPITests.Integration.Authorization;


public class RoleBasedAccessTests : IntegrationTestBase
{
    public RoleBasedAccessTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AdminEndpoint_WhenUser_ShouldReturn403()
    {
        // Arrange
        var userToken = await RegisterAndLoginAsync("regular@mail.com");
        SetAuthToken(userToken);

        // Act
        var response = await Client.GetAsync("/api/admin/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminEndpoint_WhenAdmin_ShouldReturn200()
    {
        // Arrange
        var adminToken = await LoginAsAdminAsync();
        SetAuthToken(adminToken);

        // Act
        var response = await Client.GetAsync("/api/admin/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_WhenNotAuthenticated_ShouldReturn401()
    {
        // Act
        var response = await Client.GetAsync("/api/admin/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}