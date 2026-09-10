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

namespace Application.Commands.Admin.Dashboard;

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
        if (command.FirstDayOfThePriod > command.LastDayOfThePriod)
        {
            _logger.LogWarning("firstDate - {command.FirstDayOfThePriod}, " +
                "is later than lastDate - {command.LastDayOfThePriod}.", command.FirstDayOfThePriod,
                 command.LastDayOfThePriod);

            throw new DomainException("firstDate can't be later than lastDate");
        }

        return await _orderRepository.GetNumberOfNewOrdersAsync(command.LastDayOfThePriod,
            command.FirstDayOfThePriod, ct);
    }
}
