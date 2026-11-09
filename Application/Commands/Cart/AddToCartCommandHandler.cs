using Application.Commands.User;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Cart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartCommandHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddToCartCommand request, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 1. Получаем и блокируем корзину
            var cart = await _cartRepository.GetCartForUpdateAsync(request.UserId, ct);
            if (cart == null)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new NotFoundException("Cart not found");
            }

            // 2. Получаем товар
            var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException("Product not found");
            }

            // 3. Добавляем в корзину (бизнес-логика внутри сущности)
            cart.AddItem(product, request.Quantity);

            // 4. Сохраняем
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }


}

