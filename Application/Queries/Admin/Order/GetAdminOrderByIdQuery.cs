using Application.DTOs.Order;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Order;

// ✅ Админский запрос заказа по ID (без проверки владельца)
public class GetAdminOrderByIdQuery : IRequest<OrderResponseDto?>
{
    public int OrderId { get; init; }
}
