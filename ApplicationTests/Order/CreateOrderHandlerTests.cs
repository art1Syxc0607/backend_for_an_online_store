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
        var productId = 1;
        var emailToken = "token123";
        var user = new User("test@mail.com", "hash", "John");
        user.GenerateEmailConfirmationToken(emailToken, DateTime.UtcNow + TimeSpan.FromMinutes(15));
        user.ConfirmEmail(emailToken);
        var product = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 10, "Test product", null);
        product.TestsSetProduct(productId);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Product> { product });

        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock.Setup(x => x.CreateOrder(It.IsAny<Domain.Entities.Order>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Order, CancellationToken>((order, ct) =>   // ← 2 параметра!
            {
                order.TestsSetOrder(1);
            })
            .Returns(Task.CompletedTask); // ← важно: метод возвращает Task

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
                new OrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    PriceAtPurchase = 500,
                    ProductNameAtPurchase = product.Name
                }  
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        orderRepoMock.Verify(x => x.CreateOrder(It.IsAny<Domain.Entities.Order>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new CreateOrderCommandHandler(
            Mock.Of<IOrderRepository>(),
            userRepoMock.Object,
            Mock.Of<IProductRepository>(),
            Mock.Of<IEmailService>(),
            Mock.Of<IMediator>(),
            Mock.Of<IUnitOfWork>()
        );

        var command = new CreateOrderCommand
        {
            UserId = 999,
            ShippingAddress = "ул. Ленина, 1",
            Items = new List<OrderItemDto>()
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
        var emailToken = "token123";
        user.GenerateEmailConfirmationToken(emailToken, DateTime.UtcNow + TimeSpan.FromMinutes(15));
        user.ConfirmEmail(emailToken);

        var product = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 2, "Test product");

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Product> { product });

        var handler = new CreateOrderCommandHandler(
            Mock.Of<IOrderRepository>(),
            userRepoMock.Object,
            productRepoMock.Object,
            Mock.Of<IEmailService>(),
            Mock.Of<IMediator>(),
            Mock.Of<IUnitOfWork>()
        );

        var command = new CreateOrderCommand
        {
            UserId = userId,
            ShippingAddress = "ул. Ленина, 1",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = product.Id,
                    Quantity = 5,
                    PriceAtPurchase = 500,
                    ProductNameAtPurchase = product.Name
                }  // больше чем на складе
            }
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Not enough stock*");
    }
}