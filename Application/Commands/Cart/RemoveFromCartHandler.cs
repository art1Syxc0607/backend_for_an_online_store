// 1. Удалить товар полностью
using Application.Commands.Cart;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromCartHandler(ICartRepository cartRepository, 
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveFromCartCommand request, CancellationToken ct)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, ct);
        if (cart == null) throw new NotFoundException("Cart not found");

        cart.RemoveItem(request.ProductId); // ← вызывает метод сущности
        await _unitOfWork.SaveChangesAsync(ct);
    }
}