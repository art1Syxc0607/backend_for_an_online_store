using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IReviewRepository
{
    Task AddReviewAsync(Review review, CancellationToken ct);
    Task<List<Review>> GetUserReviews(int userId, CancellationToken ct);

}
