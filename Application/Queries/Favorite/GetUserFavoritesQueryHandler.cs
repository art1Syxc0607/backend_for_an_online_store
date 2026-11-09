using Application.Common;
using Application.DTOs.Favorite;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Favorite;

public class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, PagedResult<FavoriteDto>>
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetUserFavoritesQueryHandler> _logger;

    public GetUserFavoritesQueryHandler(
        IFavoriteRepository favoriteRepository,
        IProductRepository productRepository,
        ILogger<GetUserFavoritesQueryHandler> logger)
    {
        _favoriteRepository = favoriteRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<PagedResult<FavoriteDto>> Handle(
        GetUserFavoritesQuery query,
        CancellationToken ct)
    {
        var favorites = await _favoriteRepository.GetByUserIdAsync(query.UserId, ct);

        var productIds = favorites.Select(f => f.ProductId).Distinct().ToList();
        var products = new Dictionary<int, Domain.Entities.Product>();

        if (productIds.Count > 0)
        {
            var productList = await _productRepository.GetByIdsAsync(productIds, ct);
            products = productList!.ToDictionary(p => p.Id);
        }

        var dtos = favorites
            .Select(f => new FavoriteDto
            {
                UserId = f.UserId,
                ProductId = f.ProductId,
                CreatedAt = f.CreatedAt,
                ProductName = products.TryGetValue(f.ProductId, out var p) ? p.Name : null,
                ProductPrice = products.TryGetValue(f.ProductId, out var p2) ? p2.Price : null,
                ProductImageUrl = products.TryGetValue(f.ProductId, out var p3) && p3.ImageUrls != null && p3.ImageUrls.Any()
                    ? p3.ImageUrls.First()
                    : null,
            })
            .OrderByDescending(f => f.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var totalCount = favorites.Count;

        return PagedResult<FavoriteDto>.Create(
            dtos,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}
