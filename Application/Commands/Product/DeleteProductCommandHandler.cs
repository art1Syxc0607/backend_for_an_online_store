using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
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
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task Handle(DeleteProductCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, ct);

        if (product == null) throw new DomainException("No such product");

        if(await _orderRepository.HasProductInOrdersAsync(command.Id, ct))
            throw new DomainException("Cannot delete product that has orders");

        await _productRepository.DeleteProductAsync(product, ct);
        await _unitOfWork.SaveChangesAsync();

    }
}
