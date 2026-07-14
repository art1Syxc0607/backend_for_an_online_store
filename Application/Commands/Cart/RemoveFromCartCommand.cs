using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Cart;

public class RemoveFromCartCommand : IRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
}

