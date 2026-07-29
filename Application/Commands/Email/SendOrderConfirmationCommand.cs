using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Email;

public class SendOrderConfirmationCommand : IRequest
{
    public int OrderId { get; init; }
}
