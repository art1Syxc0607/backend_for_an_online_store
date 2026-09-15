using Application.Enums;
using Domain.Enums;
using System.Text.Json.Serialization;

namespace WebApi.DTOs.Order;

public class GetUserOrderHistoryDto
{
    public OrderStatus? OrderStatus { get; set; }

    public DateTime? LastDayOfThePeriod { get; set; }
    public DateTime? FirstDayOfThePeriod { get; set; }

    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }


    public SortOrderBy OrderBy { get; set; } = SortOrderBy.DateOfCreation;

    public bool? SortDesc { get; set; } = true;
}
