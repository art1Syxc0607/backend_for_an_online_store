using Domain.Exceptions;

namespace Domain.Entities;

public class Product
{
    private List<Review> _reviews = new();
    private List<CartItem> _cartItems = new();
    private List<OrderItem> _orderItems = new();
    private List<string> _imageUrls = new();
    private List<string> _videoUrls = new();
    //private List<InventoryTransaction> _inventoryTransactions = new();

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public string? Sku { get; private set; }
    //public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Внешние ключи
    public int? CategoryId { get; private set; }

    // Навигационные свойства
    public virtual Category? Category { get; private set; }
    //public virtual Seller? Seller { get; private set; }
    public virtual IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();
    public virtual IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();
    public virtual IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
    public IReadOnlyCollection<string> ImageUrls => _imageUrls.AsReadOnly();
    public IReadOnlyCollection<string> VideoUrls => _videoUrls.AsReadOnly();
    //public virtual IReadOnlyCollection<InventoryTransaction> InventoryTransactions => _inventoryTransactions.AsReadOnly();


    public Product(string name, decimal price, int stockQuantity, string description, int? categoryId = null)
    {
        ReservedQuantity = 0;
        SetName(name);
        SetPrice(price);
        SetStock(stockQuantity);
        this.Description = description;
        //SellerId = sellerId;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Бизнес-методы
    public void UpdateDetails(string? name = null, decimal? price = null, string? description = null,
        int? StockQuantity = null, string? sku = null, string? imageUrl = null)
    {
        if (name != null) SetName(name);
        if (description != null) Description = description;
        if (price != null) SetPrice(price.Value);
        if(StockQuantity  != null) SetStock(StockQuantity.Value);

        if (sku != null) Sku = sku;
        //if (imageUrl != null) ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        StockQuantity += quantity;
        //_inventoryTransactions.Add(new InventoryTransaction(this, quantity, "Stock Increase"));
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (StockQuantity < quantity)
            throw new DomainException($"Not enough stock. Available: {StockQuantity}, Requested: {quantity}");
        StockQuantity -= quantity;
        //_inventoryTransactions.Add(new InventoryTransaction(this, -quantity, "Stock Decrease"));
        UpdatedAt = DateTime.UtcNow;
    }

    public double GetAverageRating()
    {
        if (!_reviews.Any()) return 0;
        return _reviews.Average(r => r.Rating);
    }

    public void AssignCategory(Category category)
    {
        Category = category ?? throw new DomainException("Category cannot be null.");
        CategoryId = category.Id;
    }

    public void Reserve(int quantity) // Создание заказа
        // [StockQuantity не меняется, но ReservedQuantity увеличивается]
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (AvailableQuantity < quantity)
            throw new DomainException($"Not enough available stock. Available: {AvailableQuantity}");

        ReservedQuantity += quantity;
    }

    public void ReleaseReservation(int quantity)
    // Отмена заказа → ReleaseReservation(quantity) [StockQuantity не меняется, ReservedQuantity уменьшается]
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (ReservedQuantity < quantity)
            throw new DomainException($"Cannot release more than reserved. Reserved: {ReservedQuantity}");

        ReservedQuantity -= quantity;
    }

    public void ConfirmReservation(int quantity) // Оплата заказа → ConfirmReservation(quantity) [StockQuantity уменьшается, ReservedQuantity уменьшается]
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (ReservedQuantity < quantity)
            throw new DomainException($"Cannot confirm more than reserved. Reserved: {ReservedQuantity}");

        StockQuantity -= quantity;
        ReservedQuantity -= quantity;
    }

    public void SetImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new DomainException("Image URL cannot be empty");
        ImageUrl = imageUrl;
    }

    public void ClearImageUrl()
    {
        ImageUrl = null;
    }

    // Приватные методы
    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty.");
        Name = name;
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");
        Price = price;
    }

    public void SetStock(int newStock)
    {
        if (newStock < 0)
            throw new DomainException("Stock cannot be negative");
        if (newStock < ReservedQuantity)
            throw new DomainException($"Cannot set stock below reserved quantity ({ReservedQuantity})");
        StockQuantity = newStock;
    }

    public void SetImageUrls(List<string> urls)
    {
        if (urls == null) throw new DomainException("Image URLs cannot be null");

        // Максимум 8 изображений
        if (_imageUrls.Count + urls.Count > 8)
            throw new DomainException($"Maximum 8 images allowed (current: {_imageUrls.Count})");

        _imageUrls.AddRange(urls);
    }

    public void SetVideoUrls(List<string> urls)
    {
        if (urls == null) throw new DomainException("Video URLs cannot be null");

        // Максимум 2 видео
        if (_videoUrls.Count + urls.Count > 2)
            throw new DomainException($"Maximum 2 videos allowed (current: {_videoUrls.Count})");

        _videoUrls.AddRange(urls);
    }

    public void RemoveImage(string imageUrl)
    {
        if (!_imageUrls.Remove(imageUrl))
            throw new DomainException("Image not found");
    }

    public void RemoveVideo(string videoUrl)
    {
        if (!_videoUrls.Remove(videoUrl))
            throw new DomainException("Video not found");
    }

    public void ClearAllFiles()
    {
        _imageUrls.Clear();
        _videoUrls.Clear();
    }
}

