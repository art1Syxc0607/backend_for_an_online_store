using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var cart = await _cartRepository.GetCartForUpdateAsync(command.UserId, ct);

            // Идемпотентность: очистка отсутствующей корзины - не ошибка.
            // Раньше здесь бросался DomainException, из-за чего DELETE /api/cart/clear
            // возвращал 400 на пустой/отсутствующей корзине.
            if (cart == null)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                _logger.LogInformation(
                    "Cart not found for user {UserId}; nothing to clear",
                    command.UserId);
                return;
            }

            var itemsCount = cart.Items.Count;
            cart.Clear();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Cart cleared for user {UserId}. Removed {Count} items",
                command.UserId,
                itemsCount);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
