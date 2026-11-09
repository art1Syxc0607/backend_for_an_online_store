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
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteProductCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        IOrderRepository orderRepository, IFileStorageService fileStorageService,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Handle(DeleteProductCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Lock the product row so checkout cannot reserve it while we decide
            // whether deletion is allowed.
            var product = await _productRepository.GetProductForUpdateAsync(command.Id, ct);

            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException("No such product");
            }

            _logger.LogWarning(
                "Product deletion started: ProductId {ProductId}, Name {Name}",
                product.Id,
                product.Name
            );

            if (await _orderRepository.HasProductInOrdersAsync(command.Id, ct))
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException("Cannot delete product that has orders");
            }

            // 1. Запоминаем файлы и удаляем товар из БД
            var fileUrls = product.GetAllFileUrls();

            await _productRepository.DeleteProductAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // 2. Физические файлы удаляем только после успешного коммита
            foreach (var url in fileUrls)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(url, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to delete physical file {FileUrl} after product {ProductId} deletion",
                        url, product.Id);
                }
            }

            _logger.LogWarning(
                "Product deleted: ProductId {ProductId}, Name {Name}, Time {Time}",
                product.Id,
                product.Name,
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
