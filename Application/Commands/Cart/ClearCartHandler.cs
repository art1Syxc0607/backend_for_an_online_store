using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Cart;

public class ClearCartHandler : IRequestHandler<ClearCartCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<ClearCartHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ClearCartHandler(ICartRepository cartRepository, ILogger<ClearCartHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ClearCartCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
           "Clearing cart for user {UserId}",
           command.UserId);

        var cart = await _cartRepository.GetByUserIdAsync(command.UserId, ct);

        if (cart == null)
        {
            _logger.LogWarning(
                "Cart not found for user {UserId}",
                command.UserId);
            throw new DomainException("Cart isn't found");
        }

        var itemsCount = cart.Items.Count;
        cart.Clear();

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Cart cleared for user {UserId}. Removed {Count} items",
            command.UserId,
            itemsCount);
    }
}
