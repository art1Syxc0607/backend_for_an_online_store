using Application.Enums;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Review;

public class GetAllReviewsDto
{
    public bool? IsResponded { get; set; }
    public int? ProductId { get; set; }
    public int? UserId { get; set; }
    public SortReviewBy? SortReviewBy { get; set; }
    public bool? Descending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; } // Поиск по тексту отзыва или имени пользователя
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public ReviewStatus? Status { get; set; } // Фильтрация по статусу
}