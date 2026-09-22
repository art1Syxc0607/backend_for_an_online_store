using Application.DTOs.File;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class AddReviewCommand : IRequest<int>, ICacheInvalidatingCommand
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
    public string Text { get; init; }
    public int Rating { get; init; } // 1-5 stars

    public List<FileUploadDto>? Files { get; init; }


    // ✅ Инвалидируем отзывы товара + сам товар (рейтинг изменился)
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ReviewsForProduct(ProductId),
        CacheKeys.Product(ProductId),
        CacheKeys.ProductsPrefix,  // список товаров (рейтинг)
        CacheKeys.PopularProductsPrefix
    };
}
