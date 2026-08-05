using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _dpcontext;

    public ReviewRepository(AppDbContext dpcontext)
    {
        _dpcontext = dpcontext;
    }

    public async Task<List<Review>?> GetUserReviews(int userId, CancellationToken ct)
    {
        return await _dpcontext.Reviews.Where(r => r.UserId == userId).ToListAsync(ct);
    }

    public async Task<Review?> GetReviewByIdAsync(int id, CancellationToken ct)
    {
        return await _dpcontext.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<Review>?> GetProductReviews(int productId, CancellationToken ct)
    {
        return await _dpcontext.Reviews.Where(r => r.ProductId == productId).ToListAsync(ct);
    }

    public async Task AddReviewAsync(Review review, CancellationToken ct)
    {
        await _dpcontext.Reviews.AddAsync(review, ct);
    }
}
