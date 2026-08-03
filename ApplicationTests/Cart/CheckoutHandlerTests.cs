// ApplicationTests/Commands/Order/CreateOrderHandlerTests.cs
using Application.Commands.Order;
using Application.Interfaces;
using Application.DTOs.Order;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace ApplicationTests.Commands.Order;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_ShouldCreateOrder()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");
        var product = new Domain.Entities.Product("iPhone", 999.99m, 10, "Test product");

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var orderRepoMock = new Mock<IOrderRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateOrderCommandHandler(
            orderRepoMock.Object,
            userRepoMock.Object,
            productRepoMock.Object,
            Mock.Of<IEmailService>(),
            Mock.Of<IMediator>(),
            unitOfWorkMock.Object
        );

        var command = new CreateOrderCommand
        {
            UserId = userId,
            ShippingAddress = "ул. Ленина, 1",
            Items = new List<OrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 2 , PriceAtPurchase = 500}
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        orderRepoMock.Verify(x => x.CreateOrder(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new CreateOrderHandler(
            Mock.Of<IOrderRepository>(),
            userRepoMock.Object,
            Mock.Of<IProductRepository>(),
            Mock.Of<IUnitOfWork>()
        );

        var command = new CreateOrderCommand
        {
            UserId = 999,
            ShippingAddress = "ул. Ленина, 1",
            Items = new List<CreateOrderItemDto>()
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task Handle_WhenNotEnoughStock_ShouldThrowDomainException()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");
        var product = new Product("iPhone", 999.99m, 2, "Test product");

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new CreateOrderHandler(
            Mock.Of<IOrderRepository>(),
            userRepoMock.Object,
            productRepoMock.Object,
            Mock.Of<IUnitOfWork>()
        );

        var command = new CreateOrderCommand
        {
            UserId = userId,
            ShippingAddress = "ул. Ленина, 1",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 5 } // больше чем на складе
            }
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Not enough stock*");
    }
}