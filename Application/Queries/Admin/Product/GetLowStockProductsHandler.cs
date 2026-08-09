using Application.DTOs.Admin.Product;
using Application.Interfaces;
using Application.Queries.Admin.Product;
using MediatR;

namespace Application.Queries.Admin.GetLowStockProducts;

public class GetLowStockProductsHandler : IRequestHandler<GetLowStockProductsQuery, List<LowStockProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetLowStockProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<LowStockProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken ct)
    {
        var products = await _productRepository.GetLowStockProductsAsync(
            request.Limit,
            request.IncludeReserved,
            request.CategoryId,
            request.Search,
            ct
        );

        return products.Select(p => new LowStockProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            PurchasePrice = p.PurchasePrice,
            StockQuantity = p.StockQuantity,
            ReservedQuantity = p.ReservedQuantity,
            AvailableQuantity = p.AvailableQuantity,
            CreatedAt = p.CreatedAt,
            CategoryName = p.Category?.Name,
            ImageUrls = p.ImageUrls.ToList(),
            OrdersCount = p.OrderItems?.Count ?? 0
        }).ToList();
    }
}