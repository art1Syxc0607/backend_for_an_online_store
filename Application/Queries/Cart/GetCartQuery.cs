using Application.DTOs.Cart;
using MediatR;

namespace Application.Queries.Cart;

public class GetCartQuery : IRequest<CartResponseDto>
{
    public int UserId { get; set; }
}
