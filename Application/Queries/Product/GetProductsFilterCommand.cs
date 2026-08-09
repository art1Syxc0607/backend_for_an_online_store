using Application.DTOs.Product;
using Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductsFilterCommand : IRequest<List<ProductResponseDto>>
{
    public string? SearchText { get; init; }
    public int? CategoryId { get; init; }
    public decimal? PriceLimitMax { get; init; }
    public decimal? PriceLimitMin { get; init; }
    public bool? OnlyAvailable { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public SortProductBy SortBy { get; init; } = 0;
    public bool SortDesc { get; init; } = true;
    //public bool? OnlyOutOfUserCart { get; init; } = false;


}
