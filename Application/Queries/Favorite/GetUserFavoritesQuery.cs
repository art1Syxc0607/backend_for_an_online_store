using Application.Common;
using Application.DTOs.Favorite;
using MediatR;

namespace Application.Queries.Favorite;

public class GetUserFavoritesQuery : IRequest<PagedResult<FavoriteDto>>
{
    public int UserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
