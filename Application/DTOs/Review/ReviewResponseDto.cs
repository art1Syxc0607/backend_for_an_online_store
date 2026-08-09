using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Review;

public record class ReviewResponseDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Text { get; init; }
    public int Rating { get; init; } // 1-5 stars
    public ReviewStatus Status { get; init; }
    public string? AdminResponse { get; init; }
    public DateTime? AdminResponseAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
   
}
