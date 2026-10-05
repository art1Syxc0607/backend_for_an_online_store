using Application.DTOs.Admin.Order;
using Application.Enums;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Admin.Dashboard;

public class GetRevenueForThePeriodHandler : IRequestHandler<GetRevenueForThePeriodCommand, 
    RevenueForThePeriodDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetRevenueForThePeriodHandler> _logger;

    public GetRevenueForThePeriodHandler(IOrderRepository orderRepository, 
        ILogger<GetRevenueForThePeriodHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<RevenueForThePeriodDto> Handle(GetRevenueForThePeriodCommand command, CancellationToken ct = default)
    {
        if (command.FirstDayOfThePriod > command.LastDayOfThePriod)
        {
            _logger.LogWarning("firstDate - {command.FirstDayOfThePriod}, " +
                "is later than lastDate - {command.LastDayOfThePriod}.", command.FirstDayOfThePriod,
                 command.LastDayOfThePriod);

            throw new DomainException("firstDate can't be later than lastDate");
        }

        var revenue = await _orderRepository.GetRevenueForThePeriodAsync(command.LastDayOfThePriod,
            command.FirstDayOfThePriod, ct);


        return revenue;
    }
}
