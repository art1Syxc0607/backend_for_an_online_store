using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.Cart;

// 3. Установить точное количество
public class UpdateCartItemQuantityHandler : IRequestHandler<UpdateCartItemQuantityCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCartItemQuantityHandler(ICartRepository cartRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCartItemQuantityCommand request, CancellationToken ct)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, ct);
        if (cart == null) throw new NotFoundException("Cart not found");

        if (request.NewQuantity <= 0)
        {
            cart.RemoveItem(request.ProductId); // удаляем, если 0
        }
        else
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (item == null) throw new NotFoundException("Item not found in cart");

            // зачем в корзине это делать?
            // Проверяем наличие на складе (через Product)
            //var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
            //if (product.StockQuantity < request.NewQuantity)
            //    throw new DomainException($"Not enough stock. Available: {product.StockQuantity}");

            item.SetQuantity(request.NewQuantity);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
