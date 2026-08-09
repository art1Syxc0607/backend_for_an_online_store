using Application.DTOs.Order;
using Application.Enums;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Order;

public class GetAllOrderDto
{
    public OrderStatus? OrderStatus { get; set; }
    public DateTime? Date { get; set; }
    public DateSpan? DateSpan { get; set; }
    public int? UserID { get; set; }

    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public SortOrderBy OrderBy { get; set; } = SortOrderBy.DateOfCreation;
    public bool? SortDesc { get; set; } = true;
}