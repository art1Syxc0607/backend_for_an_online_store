using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Order;

public class GetAdminOrderByIdHandler : IRequestHandler<GetAdminOrderByIdQuery, OrderResponseDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetAdminOrderByIdHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto?> Handle(GetAdminOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(request.OrderId, ct);
        if (order == null)
            return null;

        return _mapper.Map<OrderResponseDto>(order);
    }
}
