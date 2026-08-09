using Application.DTOs.Product;
using Application.Interfaces;
using MediatR;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions;

namespace Application.Queries.Product;

public class GetAllProductsCommandHandler: IRequestHandler<GetAllProductsCommand, List<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
        


    public async Task<List<ProductResponseDto>> Handle(GetAllProductsCommand command, CancellationToken ct)
    {
        var products = await _productRepository.GetAllProductsAsync(ct);

        if (products == null) throw new DomainException("No products");

        if (!products.Any())
            return new List<ProductResponseDto>();

        var productsdto = products.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ReservedQuantity = p.ReservedQuantity,
            ImageUrls = p.ImageUrls.ToList(),
            VideoUrls = p.VideoUrls.ToList(),
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        }).ToList();

        return productsdto;
    }
}
