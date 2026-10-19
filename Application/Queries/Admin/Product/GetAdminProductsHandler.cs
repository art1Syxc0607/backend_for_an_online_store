using Application.Common;
using Application.DTOs.Admin.Product;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Product;

public class GetAdminProductsHandler : IRequestHandler<GetAdminProductsQuery, PagedResult<AdminProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetAdminProductsHandler> _logger;
    private readonly IMapper _mapper;

    public GetAdminProductsHandler(
        IProductRepository productRepository,
        ILogger<GetAdminProductsHandler> logger,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PagedResult<AdminProductDto>> Handle(
        GetAdminProductsQuery request,
        CancellationToken ct)
    {
        var pageNumber = Math.Max(1, request.PageNumber ?? 1);
        var pageSize = Math.Clamp(request.PageSize ?? 20, 1, 50);

        var (items, totalCount) = await _productRepository.GetProductsFilter(
            request.CategoryId,
            request.SearchText,
            request.PriceLimitMax,
            request.PriceLimitMin,
            request.OnlyAvailable,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortDesc,
            ct);

        var mapped = items.Select(p => new AdminProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            PurchasePrice = p.PurchasePrice,
            StockQuantity = p.StockQuantity,
            ReservedQuantity = p.ReservedQuantity,
            AvailableQuantity = p.AvailableQuantity,
            AverageRating = p.GetAverageRating(),
            CountOfReviews = p.Reviews.Count,
            AmountOfRecieved = p.AmountOfReceived,
            AmountOfPaid = p.AmountOfPaid,
            AmountOfCanceled = p.AmountOfCanceled,
            CountOfOrdersContainThisProduct = p.OrderItems.Count(),
            ImageUrls = p.ImageUrls.ToList(),
            VideoUrls = p.VideoUrls.ToList(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
        }).ToList();

        return PagedResult<AdminProductDto>.Create(
            mapped,
            totalCount,
            pageNumber,
            pageSize);
    }
}
