using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly AppDbContext _context;

    public FavoriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFavoritedAsync(
        int userId,
        int productId,
        CancellationToken ct = default)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ProductId == productId, ct);
    }

    public async Task<List<Favorite>> GetByUserIdAsync(
        int userId,
        CancellationToken ct = default)
    {
        return await _context.Favorites
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Favorite favorite, CancellationToken ct = default)
    {
        await _context.Favorites.AddAsync(favorite, ct);
    }

    public async Task RemoveAsync(
        int userId,
        int productId,
        CancellationToken ct = default)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, ct);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
        }
    }
}
