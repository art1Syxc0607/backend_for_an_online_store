using MediatR;

namespace Application.Commands.Favorite;

public class RemoveFromFavoritesCommand : IRequest
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
}
