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

namespace Application.Commands.Admin.Dashboard;

public class GetMostPopularProductsForThePeriodHandler : IRequestHandler<GetMostPopularProductsForThePeriodCommand,
    List<PopularProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetMostPopularProductsForThePeriodHandler> _logger;

    public GetMostPopularProductsForThePeriodHandler(IProductRepository productRepository,
        IOrderRepository orderRepository, ICacheService cacheService,
        ILogger<GetMostPopularProductsForThePeriodHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _logger = logger;
    }


    public async Task<List<PopularProductDto>> Handle(GetMostPopularProductsForThePeriodCommand command,
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

        // 3. ✅ Формирование ключа с явным форматом
        var cacheKey = $"products:popular:" +
                       $"{firstDay:yyyyMMdd}_" +
                       $"{lastDay:yyyyMMdd}_" +
                       $"p{pageNumber}_" +
                       $"s{pageSize}";

        var cached = await _cacheService.GetAsync<List<PopularProductDto>>(cacheKey);
        if (cached != null)
            return cached;


        var result = await _productRepository.GetMostPopularProductsForThePeriod(command, ct);

        var ttl = GetCacheTtl(command.LastDayOfThePeriod);
        if (ttl > TimeSpan.Zero)
        {
            await _cacheService.SetAsync(cacheKey, result, ttl);
        }

        return result;
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
