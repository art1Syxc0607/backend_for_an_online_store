using Domain.Enums;
using Domain.Exceptions;
using Domain.DTOs.Order;

namespace Domain.Entities;

public class Order
{
    private List<OrderItem> _items = new();

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; }
    public virtual IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(User user, string shippingAddress, List<OrderItemDomainDto> items)
    {
        User = user ?? throw new DomainException("User cannot be null.");
        UserId = user.Id;

        items.ForEach(oi_dto => AddItem(oi_dto.Product, oi_dto.Quantity, oi_dto.PriceAtPurchase));

        ShippingAddress = shippingAddress ?? throw new DomainException("Address cannot be null.");
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = 0;
    }

    // Бизнес-методы
    public void AddItem(Product product, int quantity, decimal priceAtPurchase)
    {
        if (product == null) throw new DomainException("Product cannot be null.");
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to a non-pending order.");

        // ✅ Проверка доступного количества (с учетом резерва)
        if (product.AvailableQuantity < quantity)
            throw new DomainException($"Not enough stock. Available: {product.AvailableQuantity}");

        // ✅ Резервируем товар (уменьшаем доступный остаток)
        product.Reserve(quantity);

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(this, product, quantity, priceAtPurchase));
        }

        RecalculateTotal();
    }

    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
            throw new DomainException("Order is already paid.");
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Cannot pay for a cancelled order.");
        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid || Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel a paid or shipped order.");
        Status = OrderStatus.Cancelled;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException("Only paid orders can be shipped.");
        Status = OrderStatus.Shipped;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Only shipped orders can be delivered.");
        Status = OrderStatus.Delivered;
    }

    public void ReceivedByUser()
    {
        if (Status != OrderStatus.Delivered)
            throw new DomainException("Only delivered orders can be received.");
        Status = OrderStatus.Received;
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.PriceAtPurchase * i.Quantity);
    }
}