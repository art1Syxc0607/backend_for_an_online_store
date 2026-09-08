using Application.Commands.Admin.Review;
using Application.Enums;
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
    Task<Review?> GetReviewByIdAsync(int id, CancellationToken ct = default);
    Task<List<Review>> GetUserReviews(int userId, CancellationToken ct = default);
    Task<List<Review>> GetProductReviews(int productId, CancellationToken ct = default);
    Task<List<Review>> GetAllWithFiltersAsync(
           GetAllReviewsCommand command, CancellationToken ct);

}
