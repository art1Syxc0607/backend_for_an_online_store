// WebAPITests/Integration/Cart/CartClearTests.cs
using System.Net;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPITests.Infrastructure;

public class CartClearTests : IntegrationTestBase
{
    public CartClearTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ClearCart_WhenCartRowIsMissing_ReturnsNoContent()
    {
        // Arrange
        var email = "clear-cart-missing@mail.com";
        var token = await RegisterAndLoginAsync(email);
        SetAuthToken(token);

        // Удаляем корзину напрямую в БД: раньше DELETE /api/cart/clear
        // отвечал 400 ("Cart isn't found") на отсутствующую корзину.
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cart = await db.Carts.FirstAsync(c => c.User.Email == email);
            db.Carts.Remove(cart);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await Client.DeleteAsync("/api/cart/clear");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "очистка отсутствующей корзины должна быть идемпотентной");
    }

    [Fact]
    public async Task ClearCart_CalledTwice_ReturnsNoContentBothTimes()
    {
        // Arrange
        var token = await RegisterAndLoginAsync("clear-cart-twice@mail.com");
        SetAuthToken(token);

        // Act
        var first = await Client.DeleteAsync("/api/cart/clear");
        var second = await Client.DeleteAsync("/api/cart/clear");

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
