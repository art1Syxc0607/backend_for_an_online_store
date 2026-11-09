using MediatR;

namespace Application.Commands.Favorite;

public class AddToFavoritesCommand : IRequest
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
}
