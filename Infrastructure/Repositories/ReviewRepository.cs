using Application.Commands.Admin.Review;
using Application.Common;
using Application.Enums;
using Application.Interfaces;
using Application.Queries.Review;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task<PagedResult<Review>> GetProductReviewsAsync(GetProductReviewsQuery query, CancellationToken ct)
    {
        // ═══════════════════════════════════════════
        // 1. Базовый запрос
        // ═══════════════════════════════════════════
        var reviewsQuery = _dpcontext.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == query.ProductId)
            .Where(r => r.Status == ReviewStatus.Approved);  // ← только одобренные

        // ═══════════════════════════════════════════
        // 2. Фильтры
        // ═══════════════════════════════════════════

        // Точный рейтинг
        if (query.Rating.HasValue)
        {
            reviewsQuery = reviewsQuery
                .Where(r => r.Rating == query.Rating.Value);
        }

        // Минимальный рейтинг
        if (query.MinRating.HasValue)
        {
            reviewsQuery = reviewsQuery
                .Where(r => r.Rating >= query.MinRating.Value);
        }

        // Есть медиа (фото/видео)
        if (query.HasMedia.HasValue)
        {
            if (query.HasMedia.Value)
            {
                reviewsQuery = reviewsQuery
                    .Where(r => r._imageUrls.Any() || r._videoUrls.Any());
            }
            else
            {
                reviewsQuery = reviewsQuery
                    .Where(r => !r._imageUrls.Any() && !r._videoUrls.Any());
            }
        }

        // Подтверждённая покупка
        if (query.IsVerifiedPurchase.HasValue)
        {
            reviewsQuery = reviewsQuery
                .Where(r => r.IsVerifiedPurchase == query.IsVerifiedPurchase.Value);
        }

        // Есть ответ администратора
        if (query.HasAdminResponse.HasValue)
        {
            if (query.HasAdminResponse.Value)
            {
                reviewsQuery = reviewsQuery
                    .Where(r => !string.IsNullOrEmpty(r.AdminResponse));
            }
            else
            {
                reviewsQuery = reviewsQuery
                    .Where(r => string.IsNullOrEmpty(r.AdminResponse));
            }
        }

        // Диапазон дат
        if (query.FromDate.HasValue)
        {
            reviewsQuery = reviewsQuery
                .Where(r => r.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            reviewsQuery = reviewsQuery
                .Where(r => r.CreatedAt <= query.ToDate.Value);
        }

        // ═══════════════════════════════════════════
        // 3. Общее количество (ДО пагинации)
        // ═══════════════════════════════════════════
        var totalCount = await reviewsQuery.CountAsync(ct);

        // ═══════════════════════════════════════════
        // 4. Сортировка
        // ═══════════════════════════════════════════
        reviewsQuery = query.SortBy switch
        {
            ReviewSortBy.Rating => query.Descending
                ? reviewsQuery.OrderByDescending(r => r.Rating)
                               .ThenByDescending(r => r.CreatedAt)
                : reviewsQuery.OrderBy(r => r.Rating)
                               .ThenBy(r => r.CreatedAt),

            ReviewSortBy.DateOfCreation or _ => query.Descending
                ? reviewsQuery.OrderByDescending(r => r.CreatedAt)
                : reviewsQuery.OrderBy(r => r.CreatedAt)
        };

        // ═══════════════════════════════════════════
        // 5. Пагинация
        // ═══════════════════════════════════════════
        var items = await reviewsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);


        return PagedResult<Review>.Create(
            items, totalCount, query.PageNumber, query.PageSize);


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


    public async Task<bool> HasUserReviewedProductAsync(
        int userId,
        int productId,
        CancellationToken ct = default)
    {
        return await _dpcontext.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.UserId == userId
                        && r.ProductId == productId
                        && r.Status == ReviewStatus.Approved, ct);
    }
}
