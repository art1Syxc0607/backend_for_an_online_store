using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Enums;

namespace Application.DTOs.Order;

public record ProductFilterDto
{
    public string? SearchText { get; init; }
    public int? CategoryId { get; init; }
    public int? PriceLimit { get; init; }
    public bool? OnlyAvailable { get; init; } = true;
}
