using Application.DTOs.Admin.Product;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Product;

public class GetAdminProductByIdHandler : IRequestHandler<GetAdminProductByIdQuery, AdminProductDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetAdminProductByIdHandler> _logger;

    public GetAdminProductByIdHandler(
        IProductRepository productRepository,
        ILogger<GetAdminProductByIdHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<AdminProductDto?> Handle(GetAdminProductByIdQuery request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, ct);
        if (product == null)
            return null;

        return new AdminProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            PurchasePrice = product.PurchasePrice,
            StockQuantity = product.StockQuantity,
            ReservedQuantity = product.ReservedQuantity,
            AvailableQuantity = product.AvailableQuantity,
            AverageRating = product.GetAverageRating(),
            CountOfReviews = product.Reviews.Count,
            AmountOfRecieved = product.AmountOfReceived,
            AmountOfPaid = product.AmountOfPaid,
            AmountOfCanceled = product.AmountOfCanceled,
            CountOfOrdersContainThisProduct = product.OrderItems.Count(),
            ImageUrls = product.ImageUrls.ToList(),
            VideoUrls = product.VideoUrls.ToList(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
        };
    }
}
