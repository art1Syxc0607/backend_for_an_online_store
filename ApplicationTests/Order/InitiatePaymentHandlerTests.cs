using Application.Commands.Order;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.DTOs.Order;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using OrderEntity = Domain.Entities.Order;

namespace ApplicationTests.Payments;

public class InitiatePaymentHandlerTests
{
    private static OrderEntity CreateOrder(int userId = 1, int orderId = 42)
    {
        var order = new OrderEntity(userId, "Test address", new List<OrderItemDomainDto>());
        order.TestsSetOrder(id: orderId, totalAmount: 100m);
        return order;
    }

    private static InitiatePaymentHandler CreateHandler(
        Mock<IOrderRepository> orderRepository,
        Mock<IPaymentRepository> paymentRepository,
        Mock<IPaymentService> paymentService,
        Mock<IUnitOfWork> unitOfWork,
        Mock<IUserRepository>? userRepository = null)
    {
        if (userRepository == null)
        {
            userRepository = new Mock<IUserRepository>();
            userRepository
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User("user@test.com", "hash", "User"));
        }

        return new InitiatePaymentHandler(
            orderRepository.Object,
            userRepository.Object,
            paymentService.Object,
            paymentRepository.Object,
            Mock.Of<ILogger<InitiatePaymentHandler>>(),
            Mock.Of<IEmailService>(),
            Mock.Of<IConfiguration>(),
            unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenPendingPaymentExists_ShouldReuseItWithoutCallingGateway()
    {
        // Arrange
        var order = CreateOrder();
        var pending = new Payment(order, 100m, PaymentMethod.Card, "intent_existing");

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(x => x.GetOrder(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var paymentRepository = new Mock<IPaymentRepository>();
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pending);

        var paymentService = new Mock<IPaymentService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = CreateHandler(orderRepository, paymentRepository, paymentService, unitOfWork);

        // Act
        var result = await handler.Handle(
            new InitiatePaymentCommand { OrderId = 42, UserId = 1, Method = PaymentMethod.Card },
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.PaymentIntentId.Should().Be("intent_existing");

        paymentService.Verify(
            x => x.InitiatePaymentAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()),
            Times.Never);

        paymentRepository.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConcurrentRequestCreatedPendingPayment_ShouldNotInsertSecondOne()
    {
        // Arrange
        var order = CreateOrder();
        var concurrentPayment = new Payment(order, 100m, PaymentMethod.Card, "intent_concurrent");

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(x => x.GetOrder(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        orderRepository
            .Setup(x => x.GetOrderForUpdateAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var paymentRepository = new Mock<IPaymentRepository>();
        // Nothing is pending during the fast non-locking check...
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        // ...but the parallel request wins the race and commits first.
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdForUpdateAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concurrentPayment);

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.InitiatePaymentAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, PaymentIntentId = "intent_new" });

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = CreateHandler(orderRepository, paymentRepository, paymentService, unitOfWork);

        // Act
        var result = await handler.Handle(
            new InitiatePaymentCommand { OrderId = 42, UserId = 1, Method = PaymentMethod.Card },
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.PaymentIntentId.Should().Be("intent_concurrent");

        paymentRepository.Verify(
            x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()),
            Times.Never);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoPendingPayment_ShouldInsertSinglePaymentAndCommit()
    {
        // Arrange
        var order = CreateOrder();

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(x => x.GetOrder(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        orderRepository
            .Setup(x => x.GetOrderForUpdateAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var paymentRepository = new Mock<IPaymentRepository>();
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdForUpdateAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.InitiatePaymentAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, PaymentIntentId = "intent_new" });

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = CreateHandler(orderRepository, paymentRepository, paymentService, unitOfWork);

        // Act
        var result = await handler.Handle(
            new InitiatePaymentCommand { OrderId = 42, UserId = 1, Method = PaymentMethod.Card },
            CancellationToken.None);

        // Assert
        result.PaymentIntentId.Should().Be("intent_new");

        paymentRepository.Verify(
            x => x.AddAsync(
                It.Is<Payment>(p => p.TransactionId == "intent_new"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPendingPaymentUsesAnotherMethod_ShouldFailOldAndInsertNew()
    {
        // Arrange
        var order = CreateOrder();
        var oldPending = new Payment(order, 100m, PaymentMethod.SBP, "intent_old");

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(x => x.GetOrder(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        orderRepository
            .Setup(x => x.GetOrderForUpdateAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var paymentRepository = new Mock<IPaymentRepository>();
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPending);
        paymentRepository
            .Setup(x => x.GetPendingByOrderIdForUpdateAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPending);

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.InitiatePaymentAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, PaymentIntentId = "intent_new" });

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = CreateHandler(orderRepository, paymentRepository, paymentService, unitOfWork);

        // Act
        var result = await handler.Handle(
            new InitiatePaymentCommand { OrderId = 42, UserId = 1, Method = PaymentMethod.Card },
            CancellationToken.None);

        // Assert
        result.PaymentIntentId.Should().Be("intent_new");
        oldPending.Status.Should().Be(PaymentStatus.Failed);

        paymentRepository.Verify(
            x => x.AddAsync(
                It.Is<Payment>(p => p.TransactionId == "intent_new"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // One SaveChanges to flush the old attempt, one for the new payment.
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotOwnedByUser_ShouldThrow()
    {
        // Arrange
        var order = CreateOrder(userId: 2);

        var orderRepository = new Mock<IOrderRepository>();
        orderRepository
            .Setup(x => x.GetOrder(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = CreateHandler(
            orderRepository,
            new Mock<IPaymentRepository>(),
            new Mock<IPaymentService>(),
            new Mock<IUnitOfWork>());

        // Act
        Func<Task> act = async () => await handler.Handle(
            new InitiatePaymentCommand { OrderId = 42, UserId = 1, Method = PaymentMethod.Card },
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
