using Application.DTOs.Admin.Order;
using Application.Enums;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetRevenueForThePeriodHandler : IRequestHandler<GetRevenueForThePeriodCommand, 
    RevenueForThePeriodDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetRevenueForThePeriodHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<RevenueForThePeriodDto> Handle(GetRevenueForThePeriodCommand command, CancellationToken ct = default)
    {
        var revenue = await _orderRepository.GetRevenueForThePeriodAsync(command.LastDayOfThePriod,
            command.DateSpan, ct);
        var cost = await _orderRepository.GetCostOfGoodsSoldAsync(command.LastDayOfThePriod,
            command.DateSpan, ct);


        return new RevenueForThePeriodDto
        {
            Revenue = revenue,
            Income = revenue - cost
        };
    }
}
