// ApplicationTests/Commands/Cart/AddToCartHandlerTests.cs
using Application.Commands.Cart;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using System.Reflection;
using Xunit;

namespace ApplicationTests.Commands.Cart;

public class AddToCartHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductAvailable_ShouldAddToCart()
    {
        // Arrange
        var userId = 1;
        var productId = 1;
        var quantity = 2;
        var user = new Domain.Entities.User("test@mail.com", "hash", "John");
        var product = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 10, "Test product",
            (int?)null);

        product.TestsSetProduct(productId);


        var cart = new Domain.Entities.Cart(user);
        cart.Clear(); // чтобы не было null

        var cartRepoMock = new Mock<ICartRepository>();
        cartRepoMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new AddToCartCommandHandler(
            cartRepoMock.Object,
            productRepoMock.Object,
            unitOfWorkMock.Object
        );

        var command = new AddToCartCommand
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(productId);
        cart.Items.First().Quantity.Should().Be(quantity);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var cartRepoMock = new Mock<ICartRepository>();
        cartRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Cart(new User("test@mail.com", "hash", "John")));

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        var handler = new AddToCartCommandHandler(
            cartRepoMock.Object,
            productRepoMock.Object,
            Mock.Of<IUnitOfWork>()
        );

        var command = new AddToCartCommand
        {
            UserId = 1,
            ProductId = 999,
            Quantity = 1
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Product not found*");
    }
}