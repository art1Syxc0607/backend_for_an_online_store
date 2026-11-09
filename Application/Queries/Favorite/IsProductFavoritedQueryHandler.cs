using Application.Interfaces;
using MediatR;

namespace Application.Queries.Favorite;

public class IsProductFavoritedQueryHandler : IRequestHandler<IsProductFavoritedQuery, bool>
{
    private readonly IFavoriteRepository _favoriteRepository;

    public IsProductFavoritedQueryHandler(IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<bool> Handle(IsProductFavoritedQuery query, CancellationToken ct)
    {
        return await _favoriteRepository.IsFavoritedAsync(query.UserId, query.ProductId, ct);
    }
}
