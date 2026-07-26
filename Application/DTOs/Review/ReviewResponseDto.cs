using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Review;

public record class ReviewResponseDto
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
    public string Text { get; init; }
    public int Rating { get; init; } // 1-5 stars
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
   
}
