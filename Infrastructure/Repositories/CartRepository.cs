using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<Cart?> GetByUserIdForCheckoutAsync(int userId, CancellationToken ct = default)
    {
        // Lock only the cart row. Items are loaded without products: products are
        // locked separately in a deterministic order by the product repository.
        var cart = await _context.Carts
            .FromSqlInterpolated(
                $@"SELECT * FROM ""Carts"" WHERE ""UserId"" = {userId} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (cart == null)
            return null;

        await _context.Entry(cart)
            .Collection(c => c.Items)
            .LoadAsync(ct);

        return cart;
    }

    public Task UpdateAsync(Cart cart, CancellationToken ct = default)
    {
        // The cart is tracked by EF; SaveChanges will persist its changes.
        return Task.CompletedTask;
    }
}
