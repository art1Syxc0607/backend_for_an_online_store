using Application.DTOs.Order;
using Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetNumberOfNewOrdersForThePeriodCommand : IRequest<NumberOfNewOrdersForThePeriodResponseDto>
{
    public DateTime LastDayOfThePriod {  get; set; }
    public DateTime FirstDayOfThePriod { get; set; }
}
