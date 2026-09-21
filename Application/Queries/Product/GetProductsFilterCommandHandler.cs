using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductsFilterCommandHandler : IRequestHandler<GetProductsFilterCommand, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetProductsFilterCommandHandler(IProductRepository productRepository, 
        IMapper mapper,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsFilterCommand command, CancellationToken ct)
    {

        var products = await _productRepository.GetProductsFilter(
            command.CategoryId,
            command.SearchText,
            command.PriceLimitMax,
            command.PriceLimitMin,
            command.OnlyAvailable,
            command.PageNumber,
            command.PageSize,
            command.SortBy,
            command.SortDesc
        );

        var result = _mapper.Map<List<ProductResponseDto>>(products).ToList();

        return result;
    }
}
