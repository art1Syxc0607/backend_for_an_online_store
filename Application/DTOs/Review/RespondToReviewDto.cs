using System.ComponentModel.DataAnnotations;


namespace Application.DTOs.Review;

public class RespondToReviewDto
{
    [Required]
    [MaxLength(2000)]
    public string Response { get; init; } = string.Empty;
}