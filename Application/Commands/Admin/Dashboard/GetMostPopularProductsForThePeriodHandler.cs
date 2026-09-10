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
        if (command.FirstDayOfThePriod > command.LastDayOfThePriod)
        {
            _logger.LogWarning("firstDate - {command.FirstDayOfThePriod}, " +
                "is later than lastDate - {command.LastDayOfThePriod}.", command.FirstDayOfThePriod,
                 command.LastDayOfThePriod);

            throw new DomainException("firstDate can't be later than lastDate");
        }

        var cacheKey = $"products:popular:{command.FirstDayOfThePriod}_{command.LastDayOfThePriod:yyyyMMdd}";

        var cached = await _cacheService.GetAsync<List<PopularProductDto>>(cacheKey);
        if (cached != null)
            return cached;


        var result = await _productRepository.GetMostPopularProductsForThePeriod(command, ct);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }
}
