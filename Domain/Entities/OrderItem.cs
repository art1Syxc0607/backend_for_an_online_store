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

    public OrderItem(Order order, Product product, int quantity, decimal priceAtPurchase,
        decimal purchasePriceAtPurchase)
    {
        Order = order ?? throw new DomainException("Order cannot be null.");
        Product = product ?? throw new DomainException("Product cannot be null.");
        ProductId = product.Id;

        ProductNameAtPurchase = product.Name;
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