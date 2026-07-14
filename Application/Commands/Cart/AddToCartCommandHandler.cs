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

public class AddToCartCommandHandler : IRequest<AddToCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddToCartCommandHandler(
        IPasswordHasher passwordHasher,
        IProductRepository productRepository,
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddToCartCommand request, CancellationToken ct)
    {
        // 1. Получаем корзину
        var cart = await _cartRepository.GetByUserIdAsync(request.userId, ct);
        if (cart == null)
            throw new NotFoundException("Cart not found");

        // 2. Получаем товар
        var product = await _productRepository.GetByIdAsync(request.productId, ct);
        if (product == null)
            throw new NotFoundException("Product not found");

        // 3. Добавляем в корзину (бизнес-логика внутри сущности)
        cart.AddItem(product, request.countOfProduct);

        // 4. Сохраняем
        await _unitOfWork.SaveChangesAsync(ct);

    }


}

