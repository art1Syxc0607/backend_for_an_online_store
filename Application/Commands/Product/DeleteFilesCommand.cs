using Application.DTOs.Product;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class DeleteFilesCommand : IRequest<DeleteFilesResponseDto>, ICacheInvalidatingCommand
{
    public int ProductId { get; init; }
    public List<string> FileUrls { get; init; } = new(); // список URL для удаления

    // ✅ Инвалидируем весь кэш товаров
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ProductsPrefix,
        CacheKeys.PopularProductsPrefix
    };
}
