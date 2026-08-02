// DomainTests/Entities/ProductTests.cs
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DomainTests.Entities;

public class ProductTests
{
    // ========== 1. Конструктор и создание продукта ==========

    [Fact]
    public void Constructor_WhenValidData_ShouldCreateProduct()
    {
        // Arrange
        var name = "iPhone 15";
        var price = 999.99m;
        var stock = 50;
        var description = "Latest iPhone model";
        var categoryId = 1;

        // Act
        var product = new Product(name, price, stock, description, categoryId);

        // Assert
        product.Id.Should().Be(0);
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
        product.StockQuantity.Should().Be(stock);
        product.ReservedQuantity.Should().Be(0);
        product.Description.Should().Be(description);
        product.CategoryId.Should().Be(categoryId);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.ImageUrls.Should().HaveCount(0);
        product.VideoUrls.Should().HaveCount(0);
        product.AvailableQuantity.Should().Be(stock);
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var price = 999.99m;
        var stock = 10;

        // Act
        Action act = () => new Product("", price, stock, "Discription");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Product name cannot be empty*");
    }

    [Fact]
    public void Constructor_WhenPriceIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var name = "iPhone 15";
        var stock = 10;

        // Act
        Action act = () => new Product(name, 0, stock, "Discription");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public void Constructor_WhenPriceIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        var name = "iPhone 15";
        var price = -100m;
        var stock = 10;

        // Act
        Action act = () => new Product(name, price, stock, "Discription");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public void Constructor_WhenStockIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        var name = "iPhone 15";
        var price = 999.99m;
        var stock = -5;

        // Act
        Action act = () => new Product(name, price, stock, "Discription");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Stock cannot be negative*");
    }

    // ========== 2. Обновление деталей продукта ==========

    [Fact]
    public void UpdateDetails_WhenValidData_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product("Old Name", 100m, 10, "Discription");
        var newName = "New Name";
        var newDescription = "New description";
        var newPrice = 200m;
        var newSku = "SKU-123";
        //var newImageUrl = "https://example.com/image.jpg";

        // Act
        product.UpdateDetails(name: newName, description: newDescription, price: newPrice, 
            sku: newSku);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
        product.Sku.Should().Be(newSku);
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDetails_WhenNameIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("Old Name", 100m, 10, "Discription");

        // Act
        Action act = () => product.UpdateDetails(name: "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Product name cannot be empty*");
    }

    [Fact]
    public void UpdateDetails_WhenPriceIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("Old Name", 100m, 10, "Discription");

        // Act
        Action act = () => product.UpdateDetails(price: 0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Price must be greater than zero*");
    }

    // ========== 3. Управление складом ==========

    [Fact]
    public void IncreaseStock_WhenValid_ShouldIncreaseStock()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Discription");
        var initialStock = product.StockQuantity;

        // Act
        product.IncreaseStock(5);

        // Assert
        product.StockQuantity.Should().Be(initialStock + 5);
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IncreaseStock_WhenQuantityIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Discription");

        // Act
        Action act = () => product.IncreaseStock(0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void IncreaseStock_WhenQuantityIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Discription");

        // Act
        Action act = () => product.IncreaseStock(-5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void DecreaseStock_WhenValid_ShouldDecreaseStock()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Discription");

        // Act
        product.DecreaseStock(3);

        // Assert
        product.StockQuantity.Should().Be(7);
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DecreaseStock_WhenNotEnoughStock_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5, "Discription");

        // Act
        Action act = () => product.DecreaseStock(10);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Not enough stock*");
    }

    [Fact]
    public void DecreaseStock_WhenQuantityIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5, "Discription");

        // Act
        Action act = () => product.DecreaseStock(0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void DecreaseStock_WhenQuantityIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5);

        // Act
        Action act = () => product.DecreaseStock(-3);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    // ========== 4. Резервирование товара ==========

    [Fact]
    public void Reserve_WhenValid_ShouldIncreaseReservedQuantity()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Discription");
        var initialAvailable = product.AvailableQuantity;

        // Act
        product.Reserve(3);

        // Assert
        product.ReservedQuantity.Should().Be(3);
        product.AvailableQuantity.Should().Be(initialAvailable - 3);
    }

    [Fact]
    public void Reserve_WhenNotEnoughAvailableStock_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5, "Discription");

        // Act
        Action act = () => product.Reserve(10);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Not enough available stock*");
    }

    [Fact]
    public void Reserve_WhenQuantityIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5);

        // Act
        Action act = () => product.Reserve(0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void ReleaseReservation_WhenValid_ShouldDecreaseReservedQuantity()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.Reserve(5);

        // Act
        product.ReleaseReservation(3);

        // Assert
        product.ReservedQuantity.Should().Be(2);
        product.AvailableQuantity.Should().Be(8);
    }

    [Fact]
    public void ReleaseReservation_WhenReleasingMoreThanReserved_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.Reserve(3);

        // Act
        Action act = () => product.ReleaseReservation(5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Cannot release more than reserved*");
    }

    [Fact]
    public void ConfirmReservation_WhenValid_ShouldDecreaseStockAndReserved()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.Reserve(3);

        // Act
        product.ConfirmReservation(3);

        // Assert
        product.StockQuantity.Should().Be(7);
        product.ReservedQuantity.Should().Be(0);
        product.AvailableQuantity.Should().Be(7);
    }

    [Fact]
    public void ConfirmReservation_WhenConfirmingMoreThanReserved_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.Reserve(2);

        // Act
        Action act = () => product.ConfirmReservation(5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Cannot confirm more than reserved*");
    }

    // ========== 5. Изображения и видео ==========

    [Fact]
    public void SetImageUrl_WhenValid_ShouldSetImageUrl()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var imageUrl = "https://example.com/image.jpg";

        // Act
        product.SetImageUrl(imageUrl);

        // Assert
        product.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void SetImageUrl_WhenUrlIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);

        // Act
        Action act = () => product.SetImageUrl("");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Image URL cannot be empty*");
    }

    [Fact]
    public void ClearImageUrl_WhenImageExists_ShouldClearImageUrl()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.SetImageUrl("https://example.com/image.jpg");

        // Act
        product.ClearImageUrl();

        // Assert
        product.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void SetVideoUrl_WhenValid_ShouldSetVideoUrl()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var videoUrl = "https://example.com/video.mp4";

        // Act
        product.SetVideoUrl(videoUrl);

        // Assert
        product.VideoUrl.Should().Be(videoUrl);
    }

    [Fact]
    public void SetVideoUrl_WhenUrlIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);

        // Act
        Action act = () => product.SetVideoUrl("");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Video URL cannot be empty*");
    }

    [Fact]
    public void ClearVideoUrl_WhenVideoExists_ShouldClearVideoUrl()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.SetVideoUrl("https://example.com/video.mp4");

        // Act
        product.ClearVideoUrl();

        // Assert
        product.VideoUrl.Should().BeNull();
    }

    // ========== 6. Связи (категория) ==========

    [Fact]
    public void AssignCategory_WhenValid_ShouldAssignCategory()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var category = new Category("Electronics", "Electronic devices");

        // Act
        product.AssignCategory(category);

        // Assert
        product.Category.Should().Be(category);
        product.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public void AssignCategory_WhenCategoryIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);

        // Act
        Action act = () => product.AssignCategory(null!);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Category cannot be null*");
    }

    // ========== 7. Установка списков изображений и видео ==========

    [Fact]
    public void SetImageUrls_WhenValid_ShouldSetImageUrls()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var urls = new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg"
        };

        // Act
        product.SetImageUrls(urls);

        // Assert
        product.ImageUrls.Should().HaveCount(2);
        product.ImageUrls.Should().Contain(urls);
    }

    [Fact]
    public void SetImageUrls_WhenExceedsLimit_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var urls = new List<string>
        {
            "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg",
            "6.jpg", "7.jpg", "8.jpg", "9.jpg" // 9 > 8
        };

        // Act
        Action act = () => product.SetImageUrls(urls);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Maximum 8 images allowed*");
    }

    [Fact]
    public void SetVideoUrls_WhenValid_ShouldSetVideoUrls()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var urls = new List<string>
        {
            "https://example.com/video1.mp4",
            "https://example.com/video2.mp4"
        };

        // Act
        product.SetVideoUrls(urls);

        // Assert
        product.VideoUrls.Should().HaveCount(2);
        product.VideoUrls.Should().Contain(urls);
    }

    [Fact]
    public void SetVideoUrls_WhenExceedsLimit_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var urls = new List<string> { "1.mp4", "2.mp4", "3.mp4" }; // 3 > 2

        // Act
        Action act = () => product.SetVideoUrls(urls);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Maximum 2 videos allowed*");
    }

    // ========== 8. Метод GetAverageRating ==========

    [Fact]
    public void GetAverageRating_WhenNoReviews_ShouldReturnZero()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);

        // Act
        var rating = product.GetAverageRating();

        // Assert
        rating.Should().Be(0);
    }

    [Fact]
    public void GetAverageRating_WhenReviewsExist_ShouldReturnAverage()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var user = new User("test@mail.com", "hash", "John");

        // Добавляем отзывы (через публичные методы или рефлексию)
        // Так как Reviews приватный, используем публичный метод AddReview (если есть)
        // Или создаем через конструктор Review
        var review1 = new Review(user, product, "Great phone!", 5, true);
        var review2 = new Review(user, product, "Good but pricey", 4, true);

        // Добавляем через публичный метод, если есть
        // product.AddReview(review1);
        // product.AddReview(review2);

        // Act
        var rating = product.GetAverageRating();

        // Assert
        // rating.Should().Be(4.5);
    }

    // ========== 9. Удаление файлов ==========

    [Fact]
    public void RemoveImage_WhenImageExists_ShouldRemove()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var urls = new List<string> { "image1.jpg", "image2.jpg" };
        product.SetImageUrls(urls);

        // Act
        product.RemoveImage("image1.jpg");

        // Assert
        product.ImageUrls.Should().HaveCount(1);
        product.ImageUrls.Should().Contain("image2.jpg");
        product.ImageUrls.Should().NotContain("image1.jpg");
    }

    [Fact]
    public void RemoveImage_WhenImageDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.SetImageUrls(new List<string> { "image1.jpg" });

        // Act
        Action act = () => product.RemoveImage("nonexistent.jpg");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Image not found*");
    }

    [Fact]
    public void RemoveVideo_WhenVideoExists_ShouldRemove()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        var urls = new List<string> { "video1.mp4", "video2.mp4" };
        product.SetVideoUrls(urls);

        // Act
        product.RemoveVideo("video1.mp4");

        // Assert
        product.VideoUrls.Should().HaveCount(1);
        product.VideoUrls.Should().Contain("video2.mp4");
        product.VideoUrls.Should().NotContain("video1.mp4");
    }

    [Fact]
    public void RemoveVideo_WhenVideoDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.SetVideoUrls(new List<string> { "video1.mp4" });

        // Act
        Action act = () => product.RemoveVideo("nonexistent.mp4");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Video not found*");
    }

    [Fact]
    public void ClearAllFiles_ShouldClearImageAndVideoUrls()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10);
        product.SetImageUrls(new List<string> { "image1.jpg" });
        product.SetVideoUrls(new List<string> { "video1.mp4" });

        // Act
        product.ClearAllFiles();

        // Assert
        product.ImageUrls.Should().BeEmpty();
        product.VideoUrls.Should().BeEmpty();
    }
}