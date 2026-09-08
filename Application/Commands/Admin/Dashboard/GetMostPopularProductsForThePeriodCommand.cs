using Application.DTOs.Product;
using Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetMostPopularProductsForThePeriodCommand : IRequest<List<PopularProductDto>>
{
    public DateTime LastDayOfThePriod { get; set; }
    public DateTime FirstDayOfThePriod { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}
