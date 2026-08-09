using Application.Enums;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetNumberOfNewOrdersHandler : IRequestHandler<GetNumberOfNewOrdersCommand, int>
{
    private readonly IOrderRepository _orderRepository;

    public GetNumberOfNewOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<int> Handle(GetNumberOfNewOrdersCommand command, CancellationToken ct = default)
    {
        return await _orderRepository.GetNumberOfNewOrdersAsync(command.DateSpan, ct);
    }
}
