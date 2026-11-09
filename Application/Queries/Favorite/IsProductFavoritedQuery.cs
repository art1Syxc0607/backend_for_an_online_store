using MediatR;

namespace Application.Queries.Favorite;

public class IsProductFavoritedQuery : IRequest<bool>
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
}
