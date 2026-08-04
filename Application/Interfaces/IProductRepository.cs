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
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Product>?> GetByIdsAsync(List<int> Ids, CancellationToken ct = default);
    Task<List<Product>?> GetAllProductsAsync(CancellationToken ct = default);

    Task<int> AddProductAsync(Product product, CancellationToken ct = default);
    Task UpdateProductAsync(Product product, CancellationToken ct = default);

    Task DeleteProductAsync(Product product, CancellationToken ct = default);

    Task<bool> ProductExist(int id, CancellationToken ct = default);
    Task<bool> ProductsExist(List<int> Ids, CancellationToken ct = default);


    Task<List<Product>> GetProductsFilter(int? CategoryId, string? SearchText, decimal? PriceLimitMax,
        decimal? PriceLimitMin, bool? OnlyAvailable, int? pageNumber, int? pageSize, 
        SortBy? sortBy = SortBy.Name, bool SortDesc = true, CancellationToken ct = default);

}

