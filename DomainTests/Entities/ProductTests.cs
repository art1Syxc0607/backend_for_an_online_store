// DomainTests/Entities/ProductTests.cs
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DomainTests.Entities;

public class ProductTests
{
    private const string DefaultDescription = "Test product description";

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
        Action act = () => new Product("", price, stock, DefaultDescription);

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
        Action act = () => new Product(name, 0, stock, DefaultDescription);

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
        Action act = () => new Product(name, price, stock, DefaultDescription);

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
        Action act = () => new Product(name, price, stock, DefaultDescription);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Stock cannot be negative*");
    }

    [Fact]
    public void Constructor_WhenDescriptionIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var name = "iPhone 15";
        var price = 999.99m;
        var stock = 10;

        // Act
        Action act = () => new Product(name, price, stock, "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Description cannot be empty*");
    }

    // ========== 2. Обновление деталей продукта ==========

    [Fact]
    public void UpdateDetails_WhenValidData_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product("Old Name", 100m, 10, "Old description");
        var newName = "New Name";
        var newDescription = "New description";
        var newPrice = 200m;
        var newSku = "SKU-123";

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
        var product = new Product("Old Name", 100m, 10, DefaultDescription);

        // Act
        Action act = () => product.UpdateDetails(name: "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Product name cannot be empty*");
    }

    [Fact]
    public void UpdateDetails_WhenPriceIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("Old Name", 100m, 10, DefaultDescription);

        // Act
        Action act = () => product.UpdateDetails(price: 0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public void UpdateDetails_WhenDescriptionIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("Old Name", 100m, 10, DefaultDescription);

        // Act
        Action act = () => product.UpdateDetails(description: "");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Description cannot be empty*");
    }

    // ========== 3. Управление складом ==========

    [Fact]
    public void IncreaseStock_WhenValid_ShouldIncreaseStock()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.IncreaseStock(0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void IncreaseStock_WhenQuantityIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.IncreaseStock(-5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void DecreaseStock_WhenValid_ShouldDecreaseStock()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

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
        var product = new Product("iPhone", 999.99m, 5, DefaultDescription);

        // Act
        Action act = () => product.DecreaseStock(10);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Not enough stock*");
    }

    [Fact]
    public void DecreaseStock_WhenQuantityIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5, DefaultDescription);

        // Act
        Action act = () => product.DecreaseStock(0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void DecreaseStock_WhenQuantityIsNegative_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5, DefaultDescription);

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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 5, DefaultDescription);

        // Act
        Action act = () => product.Reserve(10);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Not enough available stock*");
    }

    [Fact]
    public void Reserve_WhenQuantityIsZero_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 5, DefaultDescription);

        // Act
        Action act = () => product.Reserve(0);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*positive*");
    }

    [Fact]
    public void ReleaseReservation_WhenValid_ShouldDecreaseReservedQuantity()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.Reserve(2);

        // Act
        Action act = () => product.ConfirmReservation(5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Cannot confirm more than reserved*");
    }

    //// ========== 5. Изображения и видео ==========

    //[Fact]
    //public void SetImageUrl_WhenValid_ShouldSetImageUrl()
    //{
    //    // Arrange
    //    var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
    //    var imageUrl = "https://example.com/image.jpg";

    //    // Act
    //    product.SetImageUrls(new List<string> { imageUrl});

    //    // Assert
    //    product.ImageUrl.Should().Contain(imageUrl);
    //}

    //[Fact]
    //public void SetImageUrl_WhenUrlIsEmpty_ShouldThrowDomainException()
    //{
    //    // Arrange
    //    var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

    //    // Act
    //    Action act = () => product.SetImageUrl(new List<string> { "" });

    //    // Assert
    //    act.Should().Throw<DomainException>().WithMessage("*Image URL cannot be empty*");
    //}

    //[Fact]
    //public void ClearImageUrl_WhenImageExists_ShouldClearImageUrl()
    //{
    //    // Arrange
    //    var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
    //    product.SetImageUrl("https://example.com/image.jpg");

    //    // Act
    //    product.ClearImageUrl();

    //    // Assert
    //    product.ImageUrls.Should().BeNull();
    //}

    //[Fact]
    //public void SetVideoUrl_WhenValid_ShouldSetVideoUrl()
    //{
    //    // Arrange
    //    var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
    //    var videoUrl = "https://example.com/video.mp4";

    //    // Act
    //    product.SetVideoUrls(new List<string> { videoUrl });

    //    // Assert
    //    product.ImageUrls.Should().Contain(videoUrl);
    //}

    //[Fact]
    //public void SetVideoUrl_WhenUrlIsEmpty_ShouldThrowDomainException()
    //{
    //    // Arrange
    //    var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

    //    // Act
    //    Action act = () => product.SetVideoUrls("");

    //    // Assert
    //    act.Should().Throw<DomainException>().WithMessage("*Video URL cannot be empty*");
    //}

    //[Fact]
    //public void ClearVideoUrl_WhenVideoExists_ShouldClearVideoUrl()
    //{
    //    // Arrange
    //    var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
    //    product.SetVideoUrl("https://example.com/video.mp4");

    //    // Act
    //    product.ClearVideoUrl();

    //    // Assert
    //    product.VideoUrl.Should().BeNull();
    //}

    // ========== 6. Связи (категория) ==========

    [Fact]
    public void AssignCategory_WhenValid_ShouldAssignCategory()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.AssignCategory(null!);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Category cannot be null*");
    }

    // ========== 7. Установка списков изображений и видео ==========

    [Fact]
    public void SetImageUrls_WhenValid_ShouldSetAllImageUrls()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var urls = new List<string>
        {
            "https://example.com/image1.jpg",
            "https://example.com/image2.jpg",
            "https://example.com/image3.jpg"
        };

        // Act
        product.SetImageUrls(urls);

        // Assert
        product.ImageUrls.Should().HaveCount(3);
        product.ImageUrls.Should().Contain(urls);
    }

    [Fact]
    public void SetImageUrls_WhenNewUrlsAdded_ShouldAppendToExisting()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "image1.jpg", "image2.jpg" });

        // Act
        product.SetImageUrls(new List<string> { "image3.jpg", "image4.jpg" });

        // Assert
        product.ImageUrls.Should().HaveCount(4);
        product.ImageUrls.Should().Contain(new[] { "image1.jpg", "image2.jpg", "image3.jpg", "image4.jpg" });
    }

    [Fact]
    public void SetImageUrls_WhenExceedsMaxLimit_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var urls = Enumerable.Range(1, 9).Select(i => $"image{i}.jpg").ToList(); // 9 > 8

        // Act
        Action act = () => product.SetImageUrls(urls);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Maximum 8 images allowed*");
    }

    [Fact]
    public void SetImageUrls_WhenTotalExceedsLimit_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "image1.jpg", "image2.jpg", "image3.jpg",
        "image33.jpg", "image36.jpg", "image368.jpg",});

        // Act
        Action act = () => product.SetImageUrls(new List<string> { "image4.jpg", "image5.jpg", "image6.jpg" });

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Maximum 8 images allowed*");
    }

    [Fact]
    public void SetImageUrls_WhenListIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.SetImageUrls(new List<string>());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Image URLs cannot be null or empty*");
    }

    [Fact]
    public void SetImageUrls_WhenListIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.SetImageUrls(null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Image URLs cannot be null or empty*");
    }

    // ============================================================
    // 2. УСТАНОВКА НЕСКОЛЬКИХ ВИДЕО URL (BULK SET)
    // ============================================================

    [Fact]
    public void SetVideoUrls_WhenValid_ShouldSetAllVideoUrls()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
    public void SetVideoUrls_WhenNewUrlsAdded_ShouldAppendToExisting()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetVideoUrls(new List<string> { "video1.mp4" });

        // Act
        product.SetVideoUrls(new List<string> { "video2.mp4" });

        // Assert
        product.VideoUrls.Should().HaveCount(2);
        product.VideoUrls.Should().Contain(new[] { "video1.mp4", "video2.mp4" });
    }

    [Fact]
    public void SetVideoUrls_WhenExceedsMaxLimit_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var urls = new List<string> { "video1.mp4", "video2.mp4", "video3.mp4" }; // 3 > 2

        // Act
        Action act = () => product.SetVideoUrls(urls);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Maximum 2 videos allowed*");
    }

    [Fact]
    public void SetVideoUrls_WhenListIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.SetVideoUrls(new List<string>());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Video URLs cannot be null or empty*");
    }

    // ============================================================
    // 3. УДАЛЕНИЕ НЕСКОЛЬКИХ ФАЙЛОВ (BULK REMOVE)
    // ============================================================

    [Fact]
    public void RemoveImages_WhenAllExist_ShouldRemoveAll()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "image1.jpg", "image2.jpg", "image3.jpg" });

        // Act
        product.RemoveImages(new List<string> { "image1.jpg", "image3.jpg" });

        // Assert
        product.ImageUrls.Should().HaveCount(1);
        product.ImageUrls.Should().Contain("image2.jpg");
    }

    [Fact]
    public void RemoveImages_WhenSomeNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "image1.jpg", "image2.jpg" });

        // Act
        Action act = () => product.RemoveImages(new List<string> { "image1.jpg", "nonexistent.jpg" });

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Image(s) not found*");
        product.ImageUrls.Should().HaveCount(2); // Ничего не удалилось (атомарность)
    }

    [Fact]
    public void RemoveImages_WhenListIsEmpty_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.RemoveImages(new List<string>());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*No files specified for removal*");
    }

    [Fact]
    public void RemoveVideos_WhenAllExist_ShouldRemoveAll()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetVideoUrls(new List<string> { "video1.mp4", "video2.mp4" });

        // Act
        product.RemoveVideos(new List<string> { "video1.mp4" });

        // Assert
        product.VideoUrls.Should().HaveCount(1);
        product.VideoUrls.Should().Contain("video2.mp4");
    }

    [Fact]
    public void RemoveVideos_WhenSomeNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetVideoUrls(new List<string> { "video1.mp4", "video2.mp4" });

        // Act
        Action act = () => product.RemoveVideos(new List<string> { "video1.mp4", "nonexistent.mp4" });

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Video(s) not found*");
        product.VideoUrls.Should().HaveCount(2);
    }

    // ============================================================
    // 4. ЗАМЕНА ВСЕХ ФАЙЛОВ (BULK REPLACE)
    // ============================================================

    [Fact]
    public void ReplaceImageUrls_WhenValid_ShouldReplaceAll()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "old1.jpg", "old2.jpg" });
        var newUrls = new List<string> { "new1.jpg", "new2.jpg", "new3.jpg" };

        // Act
        product.ReplaceImageUrls(newUrls);

        // Assert
        product.ImageUrls.Should().HaveCount(3);
        product.ImageUrls.Should().Contain(newUrls);
        product.ImageUrls.Should().NotContain(new[] { "old1.jpg", "old2.jpg" });
    }

    [Fact]
    public void ReplaceImageUrls_WhenExceedsLimit_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var urls = Enumerable.Range(1, 9).Select(i => $"image{i}.jpg").ToList();

        // Act
        Action act = () => product.ReplaceImageUrls(urls);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Maximum 8 images allowed*");
    }

    [Fact]
    public void ReplaceVideoUrls_WhenValid_ShouldReplaceAll()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetVideoUrls(new List<string> { "old1.mp4" });
        var newUrls = new List<string> { "new1.mp4", "new2.mp4" };

        // Act
        product.ReplaceVideoUrls(newUrls);

        // Assert
        product.VideoUrls.Should().HaveCount(2);
        product.VideoUrls.Should().Contain(newUrls);
        product.VideoUrls.Should().NotContain("old1.mp4");
    }

    // ============================================================
    // 5. ОЧИСТКА ВСЕХ ФАЙЛОВ
    // ============================================================

    [Fact]
    public void ClearAllFiles_WhenFilesExist_ShouldClearAll()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "image1.jpg", "image2.jpg" });
        product.SetVideoUrls(new List<string> { "video1.mp4", "video2.mp4" });

        // Act
        product.ClearAllFiles();

        // Assert
        product.ImageUrls.Should().BeEmpty();
        product.VideoUrls.Should().BeEmpty();
    }

    // ============================================================
    // 6. ПОЛУЧЕНИЕ ВСЕХ URL (BULK GET)
    // ============================================================

    [Fact]
    public void GetAllFileUrls_WhenFilesExist_ShouldReturnAllUrls()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var imageUrls = new List<string> { "image1.jpg", "image2.jpg" };
        var videoUrls = new List<string> { "video1.mp4" };
        product.SetImageUrls(imageUrls);
        product.SetVideoUrls(videoUrls);

        // Act
        var allUrls = product.GetAllFileUrls();

        // Assert
        allUrls.Should().HaveCount(3);
        allUrls.Should().Contain(imageUrls);
        allUrls.Should().Contain(videoUrls);
    }

    [Fact]
    public void GetAllFileUrls_WhenNoFiles_ShouldReturnEmpty()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        var allUrls = product.GetAllFileUrls();

        // Assert
        allUrls.Should().BeEmpty();
    }

    // ============================================================
    // 7. ОБНОВЛЕНИЕ ОТДЕЛЬНЫХ ФАЙЛОВ
    // ============================================================

    [Fact]
    public void UpdateImageUrl_WhenExists_ShouldUpdate()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "old.jpg", "keep.jpg" });

        // Act
        product.UpdateImageUrl("old.jpg", "new.jpg");

        // Assert
        product.ImageUrls.Should().Contain("new.jpg");
        product.ImageUrls.Should().NotContain("old.jpg");
        product.ImageUrls.Should().Contain("keep.jpg");
    }

    [Fact]
    public void UpdateImageUrl_WhenNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "keep.jpg" });

        // Act
        Action act = () => product.UpdateImageUrl("nonexistent.jpg", "new.jpg");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Image not found*");
    }

    [Fact]
    public void UpdateVideoUrl_WhenExists_ShouldUpdate()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetVideoUrls(new List<string> { "old.mp4", "keep.mp4" });

        // Act
        product.UpdateVideoUrl("old.mp4", "new.mp4");

        // Assert
        product.VideoUrls.Should().Contain("new.mp4");
        product.VideoUrls.Should().NotContain("old.mp4");
        product.VideoUrls.Should().Contain("keep.mp4");
    }

    // ========== 8. Метод GetAverageRating ==========

    [Fact]
    public void GetAverageRating_WhenNoReviews_ShouldReturnZero()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        var rating = product.GetAverageRating();

        // Assert
        rating.Should().Be(0);
    }

    [Fact]
    public void GetAverageRating_WhenReviewsExist_ShouldReturnAverage()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var user = new User("test@mail.com", "hash", "John");

        var review1 = new Review(user, product, "Great phone!", 5, true);
        var review2 = new Review(user, product, "Good but pricey", 4, true);

        // Добавляем через публичный метод, если есть
        product.AddReview(review1);
        product.AddReview(review2);

        // Act
        var rating = product.GetAverageRating();

        // Assert
        rating.Should().Be(4.5);
    }

    // ========== 9. Удаление файлов ==========

    [Fact]
    public void RemoveImage_WhenImageExists_ShouldRemove()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
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
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        product.SetImageUrls(new List<string> { "image1.jpg" });
        product.SetVideoUrls(new List<string> { "video1.mp4" });

        // Act
        product.ClearAllFiles();

        // Assert
        product.ImageUrls.Should().BeEmpty();
        product.VideoUrls.Should().BeEmpty();
    }

    // ========== 10. Добавление отзыва ==========

    [Fact]
    public void AddReview_WhenValid_ShouldAddReview()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);
        var user = new User("test@mail.com", "hash", "John");
        var review = new Review(user, product, "Great product!", 5, true);

        // Act
        product.AddReview(review);

        // Assert
        product.Reviews.Should().HaveCount(1);
        product.Reviews.Should().Contain(review);
    }

    [Fact]
    public void AddReview_WhenReviewIsNull_ShouldThrowDomainException()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, DefaultDescription);

        // Act
        Action act = () => product.AddReview(null!);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*Review cannot be null*");
    }
}