using Application.DTOs.Review;
using Application.Enums;
using Domain.Enums;
using MediatR;

namespace Application.Commands.Admin.Review;

public class GetAllReviewsCommand : IRequest<List<ReviewResponseDto>>
{
    public bool? IsResponded { get; set; }
    public int? ProductId { get; set; }
    public int? UserId { get; set; }
    public SortReviewBy? SortReviewBy { get; set; } 
    public bool? Descending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public ReviewStatus? Status { get; set; }
}