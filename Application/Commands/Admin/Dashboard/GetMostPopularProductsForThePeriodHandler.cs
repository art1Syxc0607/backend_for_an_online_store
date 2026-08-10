using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
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

    public GetMostPopularProductsForThePeriodHandler(IProductRepository productRepository,
        IOrderRepository orderRepository, ICacheService cacheService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
    }


    public async Task<List<PopularProductDto>> Handle(GetMostPopularProductsForThePeriodCommand command,
        CancellationToken ct = default)
    {

        var cacheKey = $"products:popular:{command.Span}_{command.LastDayOfThePriod:yyyyMMdd}";

        var cached = await _cacheService.GetAsync<List<PopularProductDto>>(cacheKey);
        if (cached != null)
            return cached;


        var result = await _productRepository.GetMostPopularProductsForThePeriod(command.Span,
            command.LastDayOfThePriod, ct);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }
}
