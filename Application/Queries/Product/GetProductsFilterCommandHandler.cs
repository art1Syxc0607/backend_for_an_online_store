using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
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
    private readonly ICacheService _cacheService;

    public GetProductsFilterCommandHandler(IProductRepository productRepository, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsFilterCommand command, CancellationToken ct)
    {
        // Генерируем ключ на основе параметров запроса
        var CacheKey = $"products:filter:{command.GetCacheKey()}";

        var cached = await _cacheService.GetAsync<List<ProductResponseDto>>(CacheKey);
        if (cached != null)
            return cached;

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

        var result = products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ReservedQuantity = p.ReservedQuantity,

            AverageRating = p.GetAverageRating(),
            CountOfReviews = p.Reviews.Count,

            AmountOfRecieved = p.AmountOfReceived,
            AmountOfPaid = p.AmountOfPaid,
            AmountOfCanceled = p.AmountOfCanceled,
            CountOfOrdersContainThisProduct = p.OrderItems.Count(),
            ImageUrls = p.ImageUrls.ToList(),
            VideoUrls = p.VideoUrls.ToList(),
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        }).OrderByDescending(dto => dto.CountOfOrdersContainThisProduct).ToList();

        // Кэшируем на 10 минут
        await _cacheService.SetAsync(CacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }
}
