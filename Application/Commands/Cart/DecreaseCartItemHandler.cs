using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.Cart;

public class DecreaseCartItemHandler : IRequestHandler<DecreaseCartItemCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DecreaseCartItemHandler(ICartRepository cartRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DecreaseCartItemCommand request, CancellationToken ct)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, ct);
        if (cart == null) throw new NotFoundException("Cart not found");

        cart.DecreaseItemQuantity(request.ProductId, request.Quantity);
        await _unitOfWork.SaveChangesAsync(ct);
    }


}
