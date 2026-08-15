using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Review;

public class EditReviewDto
{
    [Required(ErrorMessage = "Review ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid review ID")]
    public int ReviewId { get; init; }

    [MinLength(3, ErrorMessage = "Review must be at least 3 characters")]
    [MaxLength(2000, ErrorMessage = "Review cannot exceed 2000 characters")]
    public string? NewText { get; init; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? NewRating { get; init; }
}
