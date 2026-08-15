

using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Review;

public class AddReviewDto
{
    [Required(ErrorMessage = "Product ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid product ID")]
    public int ProductId { get; init; }

    [Required(ErrorMessage = "Review text is required")]
    [MinLength(3, ErrorMessage = "Review must be at least 3 characters")]
    [MaxLength(2000, ErrorMessage = "Review cannot exceed 2000 characters")]
    public string Text { get; init; } = string.Empty;

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; init; }

    [MaxLength(8, ErrorMessage = "Maximum 8 files allowed")]
    public List<IFormFile>? Files { get; init; }
}
