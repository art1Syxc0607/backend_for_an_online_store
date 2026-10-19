using Application.Common;
using Application.DTOs.Admin.Product;
using Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.Product;

public class GetAdminProductsQuery : IRequest<PagedResult<AdminProductDto>>
{
    public string? SearchText { get; init; }
    public int? CategoryId { get; init; }
    public decimal? PriceLimitMax { get; init; }
    public decimal? PriceLimitMin { get; init; }
    public bool? OnlyAvailable { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 20;
    public SortProductBy? SortBy { get; init; }
    public bool SortDesc { get; init; } = true;
}
