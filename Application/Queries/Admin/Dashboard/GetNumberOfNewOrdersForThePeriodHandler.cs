using Application.Common;
using Application.DTOs.Order;
using Application.Enums;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Dashboard;

public class GetNumberOfNewOrdersForThePeriodHandler : IRequestHandler<GetNumberOfNewOrdersForThePeriodCommand,
    NumberOfNewOrdersForThePeriodResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetNumberOfNewOrdersForThePeriodHandler> _logger;

    public GetNumberOfNewOrdersForThePeriodHandler(IOrderRepository orderRepository,
         ILogger<GetNumberOfNewOrdersForThePeriodHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<NumberOfNewOrdersForThePeriodResponseDto> Handle(GetNumberOfNewOrdersForThePeriodCommand command, CancellationToken ct = default)
    {
        if (!DateRange.IsValid(command.FirstDayOfThePriod, command.LastDayOfThePriod))
        {
            _logger.LogWarning(
                "Invalid period: first day {FirstDay:yyyy-MM-dd} is later than last day {LastDay:yyyy-MM-dd}",
                command.FirstDayOfThePriod.Date,
                command.LastDayOfThePriod.Date);

            throw new DomainException("First day of the period cannot be later than last day");
        }

        return await _orderRepository.GetNumberOfNewOrdersAsync(command.LastDayOfThePriod,
            command.FirstDayOfThePriod, ct);
    }
}
