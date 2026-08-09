using Application.DTOs.Product;
using Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IOrderItemRepository
{
    Task<List<PopularProductDto>> MostPopularProductsForThePeriod(DateSpan period,
        CancellationToken ct = default);
}
