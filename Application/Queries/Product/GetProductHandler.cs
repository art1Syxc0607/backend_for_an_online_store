using Application.DTOs.Product;
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


public class GetProductHandler : IRequestHandler<GetProductQuery, ProductResponseDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetProductHandler(IProductRepository productRepository, ICacheService cacheService, 
        IMapper mapper)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ProductResponseDto> Handle(GetProductQuery request, CancellationToken ct)
    {
        var cacheKey = $"product:{request.Id}";

        var cached = await _cacheService.GetAsync<ProductResponseDto>(cacheKey);
        if (cached != null)
            return cached;

        var product = await _productRepository.GetByIdAsync(request.Id, ct);
        if (product == null)
            throw new DomainException($"Product with ID {request.Id} not found");

        var result = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ReservedQuantity = product.ReservedQuantity,
            AmountOfRecieved = product.AmountOfReceived,
            AmountOfPaid = product.AmountOfPaid,
            AmountOfCanceled = product.AmountOfCanceled,
            CountOfOrdersContainThisProduct = product.OrderItems.Count(),
            ImageUrls = product.ImageUrls.ToList(),
            VideoUrls = product.VideoUrls.ToList(),
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }
}
