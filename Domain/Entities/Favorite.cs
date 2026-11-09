using Domain.Exceptions;

namespace Domain.Entities;

public class Favorite
{
    public int UserId { get; private set; }
    public int ProductId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Product Product { get; private set; } = null!;

    private Favorite() { }

    public Favorite(int userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
        CreatedAt = DateTime.UtcNow;
    }
}
