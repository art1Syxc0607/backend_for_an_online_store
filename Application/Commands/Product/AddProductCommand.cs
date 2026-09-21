using Application.DTOs.File;
using Application.Interfaces.Caching;
using MediatR;
//using Application.DTOs.Product;

namespace Application.Commands.Product;

public class AddProductCommand : IRequest<int>, ICacheInvalidatingCommand
{
    public string Name { get; init; }
    public decimal Price { get; init; }
    public decimal PurchasePrice { get; init; }
    public int StockQuantity { get; init; }
    public int? CategoryId { get; init; }
    public string Description { get; init; }

    public List<FileUploadDto>? Files { get; init; }


    // ✅ Инвалидируем весь кэш товаров
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ProductsPrefix,
        CacheKeys.PopularProductsPrefix
    };
}