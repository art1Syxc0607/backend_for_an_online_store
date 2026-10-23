using Application.Commands.Cart;
using Application.Interfaces.Caching;
using Application.Queries.Product;
using FluentAssertions;
using Xunit;

namespace ApplicationTests.Caching;

public class CacheKeyTests
{
    [Fact]
    public void GetAllProducts_DifferentPages_ShouldHaveDifferentKeys()
    {
        var firstPage = new GetAllProductsCommand { PageNumber = 1, PageSize = 20 };
        var secondPage = new GetAllProductsCommand { PageNumber = 2, PageSize = 20 };

        firstPage.CacheKey.Should().NotBe(secondPage.CacheKey);
    }

    [Fact]
    public void GetAllProducts_ShouldNormalizePageAndSize()
    {
        var defaults = new GetAllProductsCommand();
        var explicitValues = new GetAllProductsCommand { PageNumber = 1, PageSize = 20 };
        var oversized = new GetAllProductsCommand { PageNumber = 1, PageSize = 500 };
        var clamped = new GetAllProductsCommand { PageNumber = 1, PageSize = 50 };

        defaults.CacheKey.Should().Be(explicitValues.CacheKey);
        oversized.CacheKey.Should().Be(clamped.CacheKey);
    }

    [Fact]
    public void GetProductsFilter_DifferentOnlyAvailable_ShouldHaveDifferentKeys()
    {
        var onlyAvailable = new GetProductsFilterCommand { OnlyAvailable = true };
        var withoutFilter = new GetProductsFilterCommand { OnlyAvailable = false };

        onlyAvailable.CacheKey.Should().NotBe(withoutFilter.CacheKey);
    }

    [Fact]
    public void GetProductsFilter_ShouldNormalizePaginationAndSearch()
    {
        var nullPagination = new GetProductsFilterCommand { PageNumber = null, PageSize = null };
        var explicitPagination = new GetProductsFilterCommand { PageNumber = 1, PageSize = 20 };
        var upperCaseSearch = new GetProductsFilterCommand { SearchText = "IPHONE" };
        var lowerCaseSearch = new GetProductsFilterCommand { SearchText = "iphone" };

        nullPagination.CacheKey.Should().Be(explicitPagination.CacheKey);
        upperCaseSearch.CacheKey.Should().Be(lowerCaseSearch.CacheKey);
    }

    [Fact]
    public void CheckoutCommand_ShouldInvalidateProductCaches()
    {
        var command = new CheckoutCommand
        {
            UserId = 1,
            ShippingAddress = "Test address",
            BaseUrl = "http://localhost"
        };

        command.Should().BeAssignableTo<ICacheInvalidatingCommand>();
        command.CachePrefixesToInvalidate.Should().Contain(CacheKeys.ProductsPrefix);
    }

    [Fact]
    public void AddToCartCommand_ShouldInvalidateCartCache()
    {
        var command = new AddToCartCommand { ProductId = 1, Quantity = 1, UserId = 42 };

        command.Should().BeAssignableTo<ICacheInvalidatingCommand>();
        command.CachePrefixesToInvalidate.Should().Contain(CacheKeys.Cart(42));
    }
}
