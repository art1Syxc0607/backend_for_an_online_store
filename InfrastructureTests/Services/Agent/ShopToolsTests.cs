// InfrastructureTests/Services/Agent/ShopToolsTests.cs
using Application.DTOs;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using FluentAssertions;
using Infrastructure.Services.Agent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureTests.Services.Agent;

public class ShopToolsTests
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ILogger<ShopTools>> _loggerMock;
    private readonly ShopTools _shopTools;

    public ShopToolsTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ShopTools>>();

        // ✅ Мокаем IServiceScopeFactory
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IProductRepository)))
            .Returns(_productRepoMock.Object);

        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        _scopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(scopeMock.Object);

        _shopTools = new ShopTools(_scopeFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenProductsExist_ReturnsFormattedList()
    {
        // Arrange
        var products = new List<Domain.Entities.Product>
        {
            new(name: "iPhone 15", price: 99999m, purchasePrice: 99900m, stockQuantity: 10, 
            description: "iPhone 15"),

            new(name: "Samsung S24", price: 79999m, purchasePrice: 79900m, stockQuantity: 10,
            description: "Samsung S24")
        };

        _productRepoMock
            .Setup(r => r.GetAllProductsAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 10));

        // Act
        var result = await _shopTools.GetAllProductsAsync(1, 20, CancellationToken.None);

        // Assert
        result.Should().Contain("iPhone 15");
        result.Should().Contain("Samsung S24");
        result.Should().Contain("99");  // часть цены
        result.Should().Contain("Найдено товаров: 2");
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenNoProducts_ReturnsEmptyMessage()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetAllProductsAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Domain.Entities.Product>(), 20));

        // Act
        var result = await _shopTools.GetAllProductsAsync(1, 20, CancellationToken.None);

        // Assert
        result.Should().Be("Каталог товаров пуст.");
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenRepositoryThrows_ReturnsErrorMessage()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetAllProductsAsync(1, 20, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _shopTools.GetAllProductsAsync(1, 20, CancellationToken.None);

        // Assert
        result.Should().Contain("Не удалось получить");
    }

    [Fact]
    public async Task SearchProductsAsync_WhenValidQuery_ReturnsResults()
    {
        // Arrange
        var products = new List<Domain.Entities.Product>
        {
            new(name: "iPhone 15", price: 99999m, purchasePrice: 99900m, stockQuantity: 10,
            description: "iPhone 15")
        };

        _productRepoMock
            .Setup(r => r.GetProductsFilter(
                It.IsAny<int?>(),           // CategoryId
                "iPhone",                   // SearchText
                It.IsAny<decimal?>(),       // PriceLimitMax
                It.IsAny<decimal?>(),       // PriceLimitMin
                It.IsAny<bool?>(),          // OnlyAvailable
                It.IsAny<int?>(),           // pageNumber
                It.IsAny<int?>(),           // pageSize
                It.IsAny<SortProductBy?>(), // sortBy
                It.IsAny<bool>(),           // SortDesc
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 100));

        // Act
        var result = await _shopTools.SearchProductsAsync("iPhone");

        // Assert
        result.Should().Contain("iPhone 15");
    }

    [Fact]
    public async Task SearchProductsAsync_WhenEmptyQuery_ReturnsError()
    {
        // Act
        var result = await _shopTools.SearchProductsAsync("");

        // Assert
        result.Should().Contain("не может быть пустым");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenExists_ReturnsProduct()
    {
        // Arrange
        var product = new Domain.Entities.Product(name: "iPhone 15", price: 99999m, purchasePrice: 99900m,
            stockQuantity: 10, description: "iPhone 15");

        _productRepoMock
            .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _shopTools.GetProductByIdAsync(42);

        // Assert
        result.Should().Contain("iPhone 15");
        result.Should().Contain("ID: 42");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        // Act
        var result = await _shopTools.GetProductByIdAsync(999);

        // Assert
        result.Should().Contain("не найден");
    }
}