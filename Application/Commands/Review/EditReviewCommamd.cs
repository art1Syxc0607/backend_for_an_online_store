using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class EditReviewCommamd : IRequest
{
    public int ReviewId { get; set; }
    public int UserId { get; init; }
    public string? NewText { get; init; }
    public int? NewRating { get; init; } // 1-5 stars
}
