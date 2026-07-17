using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Order;
using MediatR;

namespace Application.Queries.Order;

public class GetOrderQuery : IRequest<OrderResponseDto>
{
    public int UserId { get; init; }
    public int OrderId { get; init; }
}
