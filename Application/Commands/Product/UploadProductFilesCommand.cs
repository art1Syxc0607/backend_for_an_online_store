using Application.DTOs.File;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class UploadProductFilesCommand : IRequest<List<FileUploadResponseDto>>, ICacheInvalidatingCommand
{
    public int ProductId { get; init; }
    public List<FileUploadDto> Files { get; init; } = new();

    // ✅ Инвалидируем весь кэш товаров
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ProductsPrefix,
        CacheKeys.PopularProductsPrefix
    };
}
