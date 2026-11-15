using Application.DTOs.File;
using Application.Interfaces.Caching;
using MediatR;
//using Application.DTOs.Product;

namespace Application.Commands.Admin.Product;

public class AddProductCommand : IRequest<int>, ICacheInvalidatingCommand
{
    public string Name { get; init; }
    public decimal Price { get; init; }
    public decimal PurchasePrice { get; init; }
    public int StockQuantity { get; init; }
    public int? CategoryId { get; init; }
    public string Description { get; init; }

    /// <summary>
    /// Артикул — бизнес-ключ товара. Необязателен при создании, но если задан,
    /// должен быть уникальным (без учёта регистра и пробелов по краям).
    /// </summary>
    public string? Sku { get; init; }

    public List<FileUploadDto>? Files { get; init; }


    // ✅ Инвалидируем весь кэш товаров
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ProductsPrefix,
        CacheKeys.PopularProductsPrefix
    };
}