using Application.Common;
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
        if (!DateRange.IsValid(command.FirstDayOfThePriod, command.LastDayOfThePriod))
        {
            _logger.LogWarning(
                "Invalid period: first day {FirstDay:yyyy-MM-dd} is later than last day {LastDay:yyyy-MM-dd}",
                command.FirstDayOfThePriod.Date,
                command.LastDayOfThePriod.Date);

            throw new DomainException("First day of the period cannot be later than last day");
        }

        var revenue = await _orderRepository.GetRevenueForThePeriodAsync(command.LastDayOfThePriod,
            command.FirstDayOfThePriod, ct);


        return revenue;
    }
}
