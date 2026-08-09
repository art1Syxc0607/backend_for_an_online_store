using Application.DTOs.Admin.Order;
using Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetRevenueForThePeriodCommand : IRequest<RevenueForThePeriodDto>
{
    public DateTime LastDayOfThePriod {  get; set; }
    public DateSpan DateSpan { get; set; }
}
