using MediatR;
using Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetNumberOfNewOrdersCommand : IRequest<int>
{
    public DateSpan DateSpan { get; set; }
}
