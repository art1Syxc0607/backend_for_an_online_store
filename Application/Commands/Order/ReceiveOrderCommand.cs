using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class ReceiveOrderCommand : IRequest
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
}
