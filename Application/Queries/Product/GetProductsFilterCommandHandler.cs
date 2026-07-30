using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductsFilterCommandHandler : IRequestHandler<GetProductsFilterCommand, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    //private readonly ICartRepository _cartRepository;
    //private readonly ICategoryRepository _categoryRepository;

    public GetProductsFilterCommandHandler(IProductRepository productRepository
/*        ICategoryRepository categoryRepository, ICartRepository cartRepository*/)
    {
        _productRepository = productRepository;
        //_cartRepository = cartRepository;
        //_categoryRepository = categoryRepository;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsFilterCommand command, CancellationToken ct)
    {
        var products = await _productRepository.GetProductsFilter(
            command.CategoryId,
            command.SearchText,
            command.PriceLimitMax,
            command.PriceLimitMin,
            command.OnlyAvailable,
            command.PageNumber,
            command.PageSize,
            command.SortBy,
            command.SortDesc
        );

        return products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ReservedQuantity = p.ReservedQuantity,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        }).ToList();
    }
}
