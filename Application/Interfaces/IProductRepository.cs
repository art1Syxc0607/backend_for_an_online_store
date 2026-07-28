using Application.DTOs.Product;
using Application.Enums;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Product>?> GetByIdsAsync(List<int> Ids, CancellationToken ct);
    Task<List<Product>?> GetAllProductsAsync(CancellationToken ct);

    Task<int> AddProductAsync(Product product, CancellationToken ct);
    Task UpdateProductAsync(Product product, CancellationToken ct);

    Task DeleteProductAsync(Product product, CancellationToken ct);

    Task<bool> ProductExist(int id);
    Task<bool> ProductsExist(List<int> Ids, CancellationToken ct);


    Task<List<Product>> GetProductsFilter(int? CategoryId, string? SearchText, decimal? PriceLimitMax,
        decimal? PriceLimitMin, bool? OnlyAvailable, int? pageNumber, int? pageSize, SortBy? sortBy = SortBy.Name, bool SortDesc = true);

}

