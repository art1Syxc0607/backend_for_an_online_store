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

    public GetMostPopularProductsForThePeriodHandler(IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }


    public async Task<List<PopularProductDto>> Handle(GetMostPopularProductsForThePeriodCommand command,
        CancellationToken ct = default)
    {
        var popularProductsDto = await _productRepository.GetMostPopularProductsForThePeriod(command.Span,
            command.LastDayOfThePriod, ct);
      
        return popularProductsDto;
    }
}
