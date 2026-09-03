using Domain.Enums;
using Domain.Exceptions;
using Domain.DTOs.Order;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Order
{
    private List<OrderItem> _items = new();

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public decimal TotalAmount { get; private set; } // TotalAmountOfMoney
    public string ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    // info
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; }
    public virtual IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public void TestsSetOrder(int? id = null, decimal? totalAmount = null, DateTime? createdAt = null
        , OrderStatus? status = null)
    {
        if(id != null) Id = id.Value;
        if(createdAt != null) CreatedAt = createdAt.Value;
        if(totalAmount != null) TotalAmount = totalAmount.Value;
        if(status != null) Status = status.Value;
    }

    public Order(int userId, string shippingAddress, List<OrderItemDomainDto> items)
    {
        UserId = userId;

        items.ForEach(oi_Domaindto => AddItem(oi_Domaindto.Product, oi_Domaindto.Quantity));

        ShippingAddress = shippingAddress ?? throw new DomainException("Address cannot be null.");
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Бизнес-методы
    public void AddItem(Product product, int quantity)
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
            _items.Add(new OrderItem(Id, product.Id, quantity, product.Name, product.Price, product.PurchasePrice));
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

        _items.ForEach(oi => oi.Product.ConfirmReservation(oi.Quantity)); // увеличиваем количестов 
        // покупок у продукта
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid || Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel a paid or shipped order.");


        _items.ForEach(oi => oi.Product.ReleaseReservation(oi.Quantity)); // увеличиваем количестов 
                                                                          // отмен у продукта
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException("Only paid orders can be shipped.");
        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Only shipped orders can be delivered.");
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void ReceivedByUser()
    {
        if (Status != OrderStatus.Delivered)
            throw new DomainException("Only delivered orders can be received.");

        _items.ForEach(oi => oi.Product.RecievedInOrder(oi.Quantity)); // увеличиваем количестов 
        Status = OrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.PriceAtPurchase * i.Quantity);
    }
}