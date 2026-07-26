using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
    }

    public async Task<int> Handle(AddProductCommand command, CancellationToken ct)
    {
        if (command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, ct);
            if (category == null)
                throw new DomainException($"Category {command.CategoryId} not found");
        }

        var product = new Domain.Entities.Product(command.Name, command.Price, command.StockQuantity, 
            command.Description, command.CategoryId);

        await _productRepository.AddProductAsync(product, ct);
        await _unitOfWork.SaveChangesAsync();

        return product.Id;
    }
}
