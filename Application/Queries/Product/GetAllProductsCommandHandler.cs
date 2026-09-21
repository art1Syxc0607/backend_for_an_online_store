using Application.DTOs.Product;
using Application.Interfaces;
using MediatR;
using AutoMapper;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions;

namespace Application.Queries.Product;

public class GetAllProductsCommandHandler: IRequestHandler<GetAllProductsCommand, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    private const string CacheKey = "products:all";

    public GetAllProductsCommandHandler(IProductRepository productRepository, 
        IMapper mapper,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }
        


    public async Task<List<ProductResponseDto>> Handle(GetAllProductsCommand request, CancellationToken ct)
    {
        var cached = await _cacheService.GetAsync<List<ProductResponseDto>>(CacheKey);
        if (cached != null)
            return cached;


        var products = await _productRepository.GetAllProductsAsync(ct);

        if (products == null) throw new DomainException("No products");

        if (!products.Any())
            return new List<ProductResponseDto>();

        var result = _mapper.Map<List<ProductResponseDto>>(products)
            .OrderByDescending(dto => dto.CountOfOrdersContainThisProduct).ToList();

        // Кэшируем на 10 минут
        await _cacheService.SetAsync(CacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }
}
