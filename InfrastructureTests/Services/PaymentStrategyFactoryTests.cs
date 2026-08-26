// InfrastructureTests/Services/PaymentStrategyFactoryTests.cs
using Application.Enums;
using Application.Interfaces;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Services.Payment;
using Moq;
using Xunit;

namespace InfrastructureTests.Services;

public class PaymentStrategyFactoryTests
{
    [Fact]
    public void GetStrategy_WhenMethodExists_ShouldReturnCorrectStrategy()
    {
        // Arrange
        var cardStrategyMock = new Mock<IPaymentStrategy>();
        cardStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.Card);

        var strategies = new List<IPaymentStrategy> { cardStrategyMock.Object };
        var factory = new PaymentStrategyFactory(strategies);

        // Act
        var result = factory.GetStrategy(PaymentMethod.Card);

        // Assert
        result.Should().Be(cardStrategyMock.Object);
    }

    [Fact]
    public void GetStrategy_WhenMethodDoesNotExist_ShouldThrowArgumentException()
    {
        // Arrange
        var factory = new PaymentStrategyFactory(new List<IPaymentStrategy>());

        // Act
        Action act = () => factory.GetStrategy(PaymentMethod.Card);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unsupported payment method: Card");
    }

    [Fact]
    public void Constructor_WithMultipleStrategies_ShouldBuildDictionaryCorrectly()
    {
        // Arrange
        var cardStrategyMock = new Mock<IPaymentStrategy>();
        cardStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.Card);

        var googleStrategyMock = new Mock<IPaymentStrategy>();
        googleStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.GooglePay);

        var strategies = new List<IPaymentStrategy>
        {
            cardStrategyMock.Object,
            googleStrategyMock.Object
        };

        // Act
        var factory = new PaymentStrategyFactory(strategies);

        // Assert
        factory.GetStrategy(PaymentMethod.Card).Should().Be(cardStrategyMock.Object);
        factory.GetStrategy(PaymentMethod.GooglePay).Should().Be(googleStrategyMock.Object);
    }

    [Fact]
    public void Constructor_WithDuplicateMethods_ShouldThrowArgumentException()
    {
        // Arrange
        var strategy1Mock = new Mock<IPaymentStrategy>();
        strategy1Mock.Setup(x => x.Method).Returns(PaymentMethod.Card);

        var strategy2Mock = new Mock<IPaymentStrategy>();
        strategy2Mock.Setup(x => x.Method).Returns(PaymentMethod.Card); // Дубликат!

        var strategies = new List<IPaymentStrategy>
        {
            strategy1Mock.Object,
            strategy2Mock.Object
        };

        // Act
        Action act = () => new PaymentStrategyFactory(strategies);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
} // Сводка теста: всего: 4; сбой: 0; успешно: 4; пропущено: 0; длительность: 0,8 с