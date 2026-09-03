using Domain.Exceptions;

namespace Domain.Entities;

public class OrderItem
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductNameAtPurchase { get; private set; }
    public decimal PriceAtPurchase { get; private set; }
    public decimal PurchasePriceAtPurchase { get; private set; }
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Order Order { get; private set; }
    public virtual Product Product { get; private set; }

    private OrderItem() { }

    public OrderItem(int orderId, int productId, int quantity, string productNameAtPurchase, decimal priceAtPurchase,
        decimal purchasePriceAtPurchase)
    {
        OrderId = orderId;

        ProductId = productId;


        ProductNameAtPurchase = productNameAtPurchase;
        Quantity = quantity;
        PriceAtPurchase = priceAtPurchase > 0 ? priceAtPurchase : throw new DomainException("Price must be positive.");
        CreatedAt = DateTime.UtcNow;
        PurchasePriceAtPurchase = purchasePriceAtPurchase;
    }

    public void IncreaseQuantity(int additionalQuantity)
    {
        if (additionalQuantity <= 0)
            throw new DomainException("Additional quantity must be positive.");
        Quantity += additionalQuantity;
    }
}