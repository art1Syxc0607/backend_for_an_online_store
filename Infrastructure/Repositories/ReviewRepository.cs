using Application.Commands.Admin.Review;
using Application.Enums;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
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

    public async Task<List<Review>> GetUserReviews(int userId, CancellationToken ct)
    {
        return await _dpcontext.Reviews
            .Include(r => r.User)      // ← подгружаем пользователя
            .Include(r => r.Product)   // ← подгружаем продукт
            .Where(r => r.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<Review?> GetReviewByIdAsync(int id, CancellationToken ct)
    {
        return await _dpcontext.Reviews
            .Include(r => r.User)  // ← Загружаем пользователя!
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<Review>?> GetProductReviews(int productId, CancellationToken ct)
    {
        return await _dpcontext.Reviews
            .Include(r => r.User)  // ← Загружаем пользователя!
            .Where(r => r.ProductId == productId)
            .ToListAsync(ct);
    }

    public async Task AddReviewAsync(Review review, CancellationToken ct)
    {
        await _dpcontext.Reviews.AddAsync(review, ct);
    }

    public async Task<List<Review>> GetAllWithFiltersAsync(
       GetAllReviewsCommand command, CancellationToken ct)
    {       
        var query = _dpcontext.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .WhereIf(command.IsResponded.HasValue, r => !string.IsNullOrEmpty(r.AdminResponse)
            == command.IsResponded.Value)
            .WhereIf(command.ProductId.HasValue, r => r.ProductId == command.ProductId)
            .WhereIf(command.UserId.HasValue, r => r.UserId == command.UserId)
            .WhereIf(command.Status.HasValue, r => r.Status == command.Status)
            .WhereIf(command.MinRating.HasValue, r => r.Rating >= command.MinRating)
            .WhereIf(command.MaxRating.HasValue, r => r.Rating <= command.MaxRating)
            .WhereIf(command.SearchTerm != null, r => r.Text.Contains(command.SearchTerm)
            || r.User.UserName.Contains(command.SearchTerm));
        

        var desc = command.Descending ?? true;
        var sortedQuery = query.ApplySorting(command.SortReviewBy, desc);


        var pagination = sortedQuery.Pagination(command.PageNumber, command.PageSize);

        return await pagination.ToListAsync(ct);
    }
}
