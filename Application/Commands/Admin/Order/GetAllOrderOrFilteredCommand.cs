using Application.DTOs.Order;
using Application.Enums;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Order;

public class GetAllOrderOrFilteredCommand : IRequest<List<OrderResponseDto>>
{
    public OrderStatus? Status { get; set; }
    public DateTime? Date { get; set; }
    public DateSpan? DateSpan { get; set; }
    public int? UserId { get; set; }


    public SortOrderBy? OrderSortBy { get; set; } 
    public bool? SortDesc { get; set; } = true;

    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}
