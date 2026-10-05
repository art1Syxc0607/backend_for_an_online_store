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
        if (command.FirstDayOfThePeriod > command.LastDayOfThePeriod)
        {
            _logger.LogWarning("firstDate - {command.FirstDayOfThePriod}, " +
                "is later than lastDate - {command.LastDayOfThePriod}.", command.FirstDayOfThePeriod,
                 command.LastDayOfThePeriod);

            throw new DomainException("firstDate can't be later than lastDate");
        }

        var pageNumber = command.PageNumber ?? 1;
        var pageSize = command.PageSize ?? 20;
        var firstDay = command.FirstDayOfThePeriod.Date;  // только дата, без времени
        var lastDay = command.LastDayOfThePeriod.Date;

        


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

    private static TimeSpan GetCacheTtl(DateTime lastDay)
    {
        var now = DateTime.UtcNow;

        // ✅ Если период уже закончился (прошлое) — кешируем дольше
        if (lastDay.Date < now.Date)
            return TimeSpan.FromHours(24);

        // ✅ Если период включает сегодня — кешируем меньше
        if (lastDay.Date == now.Date)
            return TimeSpan.FromMinutes(15);

        // ✅ Будущий период — не кешируем
        return TimeSpan.Zero;
    }
}
