using Domain.Exceptions;

namespace Domain.Entities;

public class Product
{
    private List<Review> _reviews = new();
    private List<CartItem> _cartItems = new();
    private List<OrderItem> _orderItems = new();
    //private List<InventoryTransaction> _inventoryTransactions = new();

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public string? Sku { get; private set; }
    public string? ImageUrl { get; private set; }
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
    //public virtual IReadOnlyCollection<InventoryTransaction> InventoryTransactions => _inventoryTransactions.AsReadOnly();


    public Product(string name, decimal price, int stockQuantity, int? sellerId = null, int? categoryId = null)
    {
        SetName(name);
        SetPrice(price);
        SetStock(stockQuantity);
        //SellerId = sellerId;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Бизнес-методы
    public void UpdateDetails(string? name = null, string? description = null, decimal? price = null, string? 
        sku = null, string? imageUrl = null)
    {
        if (name != null) SetName(name);
        if (description != null) Description = description;
        if (price != null) SetPrice(price.Value);
        if (sku != null) Sku = sku;
        if (imageUrl != null) ImageUrl = imageUrl;
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

    private void SetStock(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("Stock cannot be negative.");
        StockQuantity = quantity;
    }
}

