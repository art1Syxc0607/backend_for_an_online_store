using Application.DTOs.Product;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductsFilterCommandHandler : IRequestHandler<GetProductsFilterCommand, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetProductsFilterCommandHandler(IProductRepository productRepository,
        ICategoryRepository categoryRepository, ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsFilterCommand command, CancellationToken ct)
    {
        if (command.CategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistByIdAsync(command.CategoryId.Value, ct))
                throw new DomainException("No such Category");
        }

        var filteredProducts = await _productRepository.GetProductsFilter(command.CategoryId.Value,
            command.SearchText, command.PriceLimit, command.OnlyAvailable, command.SortBy, command.SortDesc);


    }
}
