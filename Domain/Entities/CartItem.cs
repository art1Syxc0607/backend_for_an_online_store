using Domain.Exceptions;

namespace Domain.Entities;

public class CartItem
{
    public int Id { get; private set; }
    public int CartId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; } // в корзине может быть любое колво, это не склад

    public virtual Cart Cart { get; private set; }
    public virtual Product Product { get; private set; }

    private CartItem() { }

    public CartItem(Cart cart, Product product, int quantity)
    {
        Cart = cart ?? throw new DomainException("Cart cannot be null.");
        Product = product ?? throw new DomainException("Product cannot be null.");
        CartId = cart.Id;
        ProductId = product.Id;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int additionalQuantity) // Increase By a certain quantity
    {
        if (additionalQuantity <= 0)
            throw new DomainException("Additional quantity must be positive.");
        Quantity += additionalQuantity;
    }

    public void DecreaseQuantity(int decreaseBy) // Decrease By a certain quantity
    {
        if (decreaseBy <= 0)
            throw new DomainException("Quantity to decrease must be positive.");
        if (Quantity < decreaseBy)
            throw new DomainException("Cannot decrease below zero.");
        Quantity -= decreaseBy;
    }

    // Новый метод: обновить количество до конкретного значения
    public void SetQuantity(int newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainException("Quantity cannot be negative.");
        if (newQuantity == 0)
            throw new DomainException("Use RemoveItem to delete item entirely.");
        Quantity = newQuantity;
    }
}