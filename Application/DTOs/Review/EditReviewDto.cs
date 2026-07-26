using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Review;

public class EditReviewDto
{
    public int RevieweId { get; init; }
    public string? NewText { get; init; }
    public int? NewRating { get; init; } // 1-5 stars
}
