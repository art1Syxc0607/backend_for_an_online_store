using Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Admin.Product;

public class AdminProductFilterDto
{
    public string? SearchText { get; set; }
    public int? CategoryId { get; set; }
    public decimal? PriceLimitMin { get; set; }
    public decimal? PriceLimitMax { get; set; }
    public bool? OnlyAvailable { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SortProductBy SortBy { get; set; } = SortProductBy.Name;

    public bool SortDesc { get; set; } = true;
}
