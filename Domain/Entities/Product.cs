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
    public int ReservedQuantity { get; private set; } = 0;
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


    public Product(string name, decimal price, int stockQuantity, string description, 
        int? categoryId = null)
    {
        SetName(name);
        SetPrice(price);
        SetStock(stockQuantity);
        if (description == "") throw new DomainException("Description cannot be empty.");
        this.Description = description;
        //SellerId = sellerId;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Бизнес-методы
    public void TestsSetProduct(int? id = null)
    {
        if(id != null) Id = id.Value;
    }
    public void UpdateDetails(string? name = null, decimal? price = null, string? description = null,
        int? StockQuantity = null, string? sku = null, string? imageUrl = null)
    {
        if (name != null) SetName(name);
        if (description != null) 
        {
            if (description == "") throw new DomainException("Description cannot be empty.");
            Description = description;
        } 
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

    public void AddReview(Review review)
    {
        if (review == null) throw new DomainException("Review cannot be null");
        _reviews.Add(review);
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

    // работа с файлами
    // ========== ОБНОВЛЕНИЕ ОДНОГО URL ==========
    public void UpdateImageUrl(string oldUrl, string newUrl)
    {
        // 1. Проверка входных данных
        if (string.IsNullOrWhiteSpace(oldUrl))
            throw new DomainException("Old image URL cannot be empty");
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new DomainException("New image URL cannot be empty");

        // 2. Проверка, что старый URL существует
        if (!_imageUrls.Contains(oldUrl))
            throw new DomainException($"Image not found: {oldUrl}");

        // 3. Проверка, что новый URL не дублирует существующий (кроме самого себя)
        if (_imageUrls.Contains(newUrl) && newUrl != oldUrl)
            throw new DomainException($"Image URL already exists: {newUrl}");

        // 4. Заменяем
        var index = _imageUrls.IndexOf(oldUrl);
        _imageUrls[index] = newUrl;
    }

    public void UpdateVideoUrl(string oldUrl, string newUrl)
    {
        if (string.IsNullOrWhiteSpace(oldUrl))
            throw new DomainException("Old video URL cannot be empty");
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new DomainException("New video URL cannot be empty");

        if (!_videoUrls.Contains(oldUrl))
            throw new DomainException($"Video not found: {oldUrl}");

        if (_videoUrls.Contains(newUrl) && newUrl != oldUrl)
            throw new DomainException($"Video URL already exists: {newUrl}");

        var index = _videoUrls.IndexOf(oldUrl);
        _videoUrls[index] = newUrl;
    }

    // ========== УДАЛЕНИЕ ОДНОГО URL ==========
    public void RemoveImage(string url)
    {
        // 1. Проверка входных данных
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL cannot be empty");

        // 2. Проверка, что URL существует
        if (!_imageUrls.Remove(url))
            throw new DomainException($"Image not found: {url}");
    }

    public void RemoveVideo(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Video URL cannot be empty");

        if (!_videoUrls.Remove(url))
            throw new DomainException($"Video not found: {url}");
    }

    // ========== МАССОВАЯ ЗАГРУЗКА ==========
    public void SetImageUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Image URLs cannot be null or empty");

        // ✅ Фильтруем дубликаты
        var newUrls = urls.Distinct().ToList();
        var existingUrls = _imageUrls.ToHashSet(); // так лучше алгоритмически?

        // ✅ Проверяем, есть ли дубликаты с существующими
        var duplicates = newUrls.Where(u => existingUrls.Contains(u)).ToList();
        if (duplicates.Any())
            throw new DomainException($"Duplicate image URLs found: {string.Join(", ", duplicates)}");

        // ✅ Проверяем лимит (с учетом новых уникальных)
        if (_imageUrls.Count + newUrls.Count > 8)
            throw new DomainException($"Maximum 8 images allowed (current: {_imageUrls.Count}, adding: {newUrls.Count})");

        _imageUrls.AddRange(newUrls);
    }

    public void SetVideoUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Video URLs cannot be null or empty");

        var newUrls = urls.Distinct().ToList();
        var existingUrls = _videoUrls.ToHashSet();

        var duplicates = newUrls.Where(u => existingUrls.Contains(u)).ToList();
        if (duplicates.Any())
            throw new DomainException($"Duplicate video URLs found: {string.Join(", ", duplicates)}");

        if (_videoUrls.Count + newUrls.Count > 2)
            throw new DomainException($"Maximum 2 videos allowed (current: {_videoUrls.Count}, adding: {newUrls.Count})");

        _videoUrls.AddRange(newUrls);
    }

    // ========== МАССОВОЕ УДАЛЕНИЕ ==========
    public void RemoveImages(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("No files specified for removal");

        // ✅ Проверяем, что все URL существуют
        var missing = urls.Where(u => !_imageUrls.Contains(u)).ToList();
        if (missing.Any())
            throw new DomainException($"Image(s) not found: {string.Join(", ", missing)}");

        // ✅ Удаляем все (убираем дубли в запросе)
        var toRemove = urls.Distinct().ToList();
        foreach (var url in toRemove)
        {
            _imageUrls.Remove(url);
        }
    }

    public void RemoveVideos(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("No files specified for removal");

        var missing = urls.Where(u => !_videoUrls.Contains(u)).ToList();
        if (missing.Any())
            throw new DomainException($"Video(s) not found: {string.Join(", ", missing)}");

        var toRemove = urls.Distinct().ToList();
        foreach (var url in toRemove)
        {
            _videoUrls.Remove(url);
        }
    }

    // ========== МАССОВАЯ ЗАМЕНА ==========
    public void ReplaceImageUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Image URLs cannot be null or empty");

        var uniqueUrls = urls.Distinct().ToList();
        if (uniqueUrls.Count > 8)
            throw new DomainException($"Maximum 8 images allowed (received: {uniqueUrls.Count})");

        _imageUrls.Clear();
        _imageUrls.AddRange(uniqueUrls);
    }

    public void ReplaceVideoUrls(List<string> urls)
    {
        if (urls == null || !urls.Any())
            throw new DomainException("Video URLs cannot be null or empty");

        var uniqueUrls = urls.Distinct().ToList();
        if (uniqueUrls.Count > 2)
            throw new DomainException($"Maximum 2 videos allowed (received: {uniqueUrls.Count})");

        _videoUrls.Clear();
        _videoUrls.AddRange(uniqueUrls);
    }

    // ========== ПОЛУЧЕНИЕ ВСЕХ URL ==========
    public List<string> GetAllFileUrls()
    {
        var all = new List<string>();
        all.AddRange(_imageUrls);
        all.AddRange(_videoUrls);
        return all;
    }

    // очистка
    public void ClearImageUrls()
    {
        _imageUrls.Clear();
    }

    public void ClearVideoUrls()
    {
        _videoUrls.Clear();
    }

    public void ClearAllFiles()
    {
        _imageUrls.Clear();
        _videoUrls.Clear();
    }
}

