using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Review;

public class GetProductReviewsDto
{
    public int? Rating { get; set; }
    public int? MinRating { get; set; }
    public bool? HasMedia { get; set; }
    public bool? IsVerifiedPurchase { get; set; }
    public bool? HasAdminResponse { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public ReviewSortBy SortBy { get; set; } = ReviewSortBy.DateOfCreation;
    public bool Descending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}