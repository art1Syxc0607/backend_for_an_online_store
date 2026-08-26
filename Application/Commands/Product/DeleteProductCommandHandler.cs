using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AddProductCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        IOrderRepository orderRepository, ILogger<AddProductCommandHandler> logger,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, ct);

        if (product == null) throw new DomainException("No such product");

        _logger.LogWarning(
            "Product deletion started: ProductId {ProductId}, Name {Name}",
            product.Id,
            product.Name
        );

        if (await _orderRepository.HasProductInOrdersAsync(command.Id, ct))
            throw new DomainException("Cannot delete product that has orders");

        await _productRepository.DeleteProductAsync(product, ct);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Product deleted: ProductId {ProductId}, Name {Name}, Time {Time}",
            product.Id,
            product.Name,
            DateTime.UtcNow
        );

        // ✅ Удаляем из кэша всё
        await _cacheService.RemoveByPrefix("products:");

    }
}
