using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Order;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CancelOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order cancellation started: OrderId {OrderId}, UserId {UserId}",
            command.OrderId,
            command.UserId
        );

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Lock order first, then products in ascending Id order.
            var order = await _orderRepository.GetOrderForUpdateAsync(command.OrderId, ct);
            if (order == null)
                throw new DomainException("Order not found");

            if (order.UserId != command.UserId)
                throw new DomainException("This order doesn't belong to this user");

            var productIds = order.Items
                .Select(i => i.ProductId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var products = await _productRepository.GetByIdsForUpdateAsync(productIds, ct);
            if (products.Count != productIds.Count)
                throw new DomainException("Some products from the order were not found");

            order.Cancel();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Order cancelled successfully: OrderId {OrderId}, UserId {UserId}, Status {Status}, Time {Time}",
                order.Id,
                command.UserId,
                order.Status,
                DateTime.UtcNow
            );
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
