using Application.Common;
using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Dashboard;

public class GetMostPopularProductsForThePeriodHandler
    : IRequestHandler<GetMostPopularProductsForThePeriodCommand, 
        PagedResult<PopularProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetMostPopularProductsForThePeriodHandler> _logger;

    public GetMostPopularProductsForThePeriodHandler(IProductRepository productRepository,
        IOrderRepository orderRepository,
        ILogger<GetMostPopularProductsForThePeriodHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }


    public async Task<PagedResult<PopularProductDto>> Handle(GetMostPopularProductsForThePeriodCommand command,
        CancellationToken ct = default)
    {
        if (!DateRange.IsValid(command.FirstDayOfThePeriod, command.LastDayOfThePeriod))
        {
            _logger.LogWarning(
                "Invalid period: first day {FirstDay:yyyy-MM-dd} is later than last day {LastDay:yyyy-MM-dd}",
                command.FirstDayOfThePeriod.Date,
                command.LastDayOfThePeriod.Date);

            throw new DomainException("First day of the period cannot be later than last day");
        }

        var pageNumber = command.PageNumber ?? 1;
        var pageSize = command.PageSize ?? 20;

        var result = await _productRepository.
            GetMostPopularProductsForThePeriodAsync
            (command, ct);

        // 4. ✅ Формируем PagedResult
        return PagedResult<PopularProductDto>.Create(
            result.Items,
            result.TotalCount,
            pageNumber,
            pageSize);
    }

}
