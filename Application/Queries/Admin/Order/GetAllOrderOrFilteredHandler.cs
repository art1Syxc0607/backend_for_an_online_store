using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Order;

public class GetAllOrderOrFilteredHandler : IRequestHandler<GetAllOrderOrFilteredCommand, 
    List<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetAllOrderOrFilteredHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderResponseDto>> Handle(GetAllOrderOrFilteredCommand command, 
        CancellationToken ct)
    {
        var orders = await _orderRepository.GetOrdersFilterAsync(command, ct);

        // Маппим через AutoMapper
        var result = _mapper.Map<List<OrderResponseDto>>(orders);

        return result;

    }
}
