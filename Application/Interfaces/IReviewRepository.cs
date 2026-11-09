using Application.Common;
using Application.Queries.Admin.Review;
using Application.Queries.Review;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IReviewRepository
{
    Task AddReviewAsync(Review review, CancellationToken ct = default);
    Task DeleteReviewAsync(Review review, CancellationToken ct = default);
    Task<Review?> GetReviewByIdAsync(int id, CancellationToken ct = default);
    Task<List<Review>> GetUserReviews(int userId, CancellationToken ct = default);
    Task<PagedResult<Review>> GetProductReviewsAsync(GetProductReviewsQuery query, CancellationToken ct = default);
    Task<(List<Review> Items, int TotalCount)> GetAllWithFiltersAsync(
           GetAllReviewsCommand command, CancellationToken ct);


    Task<bool> HasUserReviewedProductAsync(
        int userId,
        int productId,
        CancellationToken ct = default);

    /// <summary>
    /// Locks a single review row for the duration of the current transaction.
    /// Returns null when the review does not exist.
    /// </summary>
    Task<Review?> GetReviewForUpdateAsync(int id, CancellationToken ct = default);
}
