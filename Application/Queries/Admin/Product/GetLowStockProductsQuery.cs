using MediatR;
using Application.DTOs.Admin.Product;

namespace Application.Queries.Admin.Product;

public class GetLowStockProductsQuery : IRequest<List<LowStockProductDto>>
{
    public int Limit { get; init; }           // Максимальное количество на складе для попадания в список
    public bool IncludeReserved { get; init; } = true; // Учитывать ли резерв
    public int? CategoryId { get; init; }     // Фильтр по категории
    public string? Search { get; init; }      // Поиск по названию
}
