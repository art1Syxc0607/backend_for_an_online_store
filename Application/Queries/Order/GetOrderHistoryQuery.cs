using Application.DTOs.Order;
using Application.Enums;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using System.Threading.Tasks;

namespace Application.Queries.Order;

public class GetOrderHistoryQuery : IRequest<List<OrderResponseDto>>
{
    public int UserId { get; init; }

    public OrderStatus? Status { get; set; }
    public DateTime? Date { get; set; }
    public DateSpan? DateSpan { get; set; }

    public SortOrderBy? OrderSortBy { get; set; }
    public bool? SortDesc { get; set; } = true;

    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}
