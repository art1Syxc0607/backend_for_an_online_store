using Domain.Exceptions;

namespace Domain.Entities;

public class Cart
{
    private List<CartItem> _items = new();

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; }
    public virtual IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public Cart(User user)
    {
        User = user ?? throw new DomainException("User cannot be null.");
        UserId = user.Id;
        UpdatedAt = DateTime.UtcNow;
    }

    // Бизнес-методы
    public void AddItem(Product product, int quantity) // if there's this product in the cart it increases quantity
        // else adds this product into the curt
    {
        if (product == null) throw new DomainException("Product cannot be null.");
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        if (product.StockQuantity < quantity)
            throw new DomainException($"Not enough stock. Available: {product.StockQuantity}"); // ?, this is just cart

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(this, product, quantity));
        }
        UpdatedAt = DateTime.UtcNow;
    }
    // 1. Удалить ВЕСЬ товар из корзины
    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) throw new DomainException("Item not found in cart.");
        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    // 2. Уменьшить КОЛИЧЕСТВО товара (на 1 или на указанное число)
    public void DecreaseItemQuantity(int productId, int quantity = 1)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException("Item not found in cart.");

        // Если количество становится 0 или меньше — удаляем товар полностью
        if (item.Quantity <= quantity)
        {
            _items.Remove(item);
        }
        else
        {
            item.DecreaseQuantity(quantity); // ← метод в CartItem
        }

        UpdatedAt = DateTime.UtcNow;
    }

    // обновить количество до конкретного значения
    public void SetQuantity(int productId, int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be positive.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException("Item not found in cart.");

        item.SetQuantity(newQuantity);

    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetTotalPrice()
    {
        return _items.Sum(i => i.Product.Price * i.Quantity);
    }
}