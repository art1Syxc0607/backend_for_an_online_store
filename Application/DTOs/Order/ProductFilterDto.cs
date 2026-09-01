using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Enums;
using System.Text.Json.Serialization;

namespace Application.DTOs.Order;

public record ProductFilterDto
{
    public string? SearchText { get; init; }
    public int? CategoryId { get; init; }
    public decimal? PriceLimitMin { get; init; }
    public decimal? PriceLimitMax { get; init; }
    public bool? OnlyAvailable { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // чтобы Swagger отображал строковые значения enum'а!
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SortProductBy SortBy { get; init; } = SortProductBy.Name;

    public bool SortDesc { get; init; } = true;
}
